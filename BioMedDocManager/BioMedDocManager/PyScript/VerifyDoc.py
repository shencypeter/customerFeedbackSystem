# 安裝套件：pip install olefile
# 用法：python D:\系統資料\project\itriDoc\BioMedDocManager\BioMedDocManager\PyScript\VerifyDoc.py "C:\Users\peter\OneDrive\Desktop\GMP文件庫(合併與重新命名)" --csv "C:\Users\peter\OneDrive\Desktop\doc驗證結果.csv" --count-docm-as-docx

# -*- coding: utf-8 -*-
import argparse
import os
from pathlib import Path
import csv
import zipfile

# 可更準確辨識舊版 .doc（OLE 結構內要有 WordDocument）
try:
    import olefile  # pip install olefile
    HAS_OLEFILE = True
except Exception:
    HAS_OLEFILE = False

ZIP_MAGIC = b"PK\x03\x04"
ZIP_EMPTY_MAGIC = b"PK\x05\x06"  # 空 ZIP
OLE_MAGIC = b"\xD0\xCF\x11\xE0\xA1\xB1\x1A\xE1"

def sniff_magic(path: Path) -> str:
    """檔頭偵測：zip / ole / rtf / unknown"""
    try:
        with open(path, "rb") as f:
            head8 = f.read(8)
            f.seek(0)
            head200 = f.read(200).lstrip()
    except Exception:
        return "unknown"
    if head8.startswith(ZIP_MAGIC) or head8.startswith(ZIP_EMPTY_MAGIC):
        return "zip"
    if head8.startswith(OLE_MAGIC):
        return "ole"
    if head200.startswith(b"{\\rtf"):
        return "rtf"
    return "unknown"

def is_real_docx_or_docm(path: Path) -> tuple[bool, str]:
    """
    檢查是否為真 DOCX/DOCM：
      - ZIP 結構
      - 內含 [Content_Types].xml 與 word/document.xml
      - Main ContentType 判定為 DOCX 或 DOCM
    """
    try:
        with zipfile.ZipFile(path, "r") as zf:
            names = set(zf.namelist())
            if "[Content_Types].xml" not in names or "word/document.xml" not in names:
                return False, "缺少必要檔"
            data = zf.read("[Content_Types].xml").decode("utf-8", errors="ignore")
    except Exception as e:
        return False, f"ZIP 解析失敗：{e}"

    if "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml" in data:
        return True, "DOCX"
    if "application/vnd.ms-word.document.macroEnabled.main+xml" in data:
        return True, "DOCM"
    return False, "不是 Word OOXML"

def is_ole_word_doc(path: Path) -> tuple[bool, str]:
    """檢查 OLE 檔是否為舊版 Word .doc（需 olefile；無則回報不確定）。"""
    if not HAS_OLEFILE:
        return False, "OLE(未知，未安裝 olefile)"
    try:
        if not olefile.isOleFile(str(path)):
            return False, "不是有效 OLE"
        with olefile.OleFileIO(str(path)) as ole:
            if ole.exists("WordDocument"):
                return True, "DOC(legacy)"
            return False, "OLE 但無 WordDocument（可能是 XLS/PPT）"
    except Exception as e:
        return False, f"OLE 解析失敗：{e}"

def classify(path: Path) -> tuple[str, str, bool]:
    """
    回傳 (Kind, Note, IsDocxLike)
    Kind: DOCX / DOCM / DOC(legacy) / RTF / OLE-OTHER / NOT-WORD / UNKNOWN
    IsDocxLike: 只在 DOCX/DOCM 為 True（可由參數調整）
    """
    magic = sniff_magic(path)
    if magic == "zip":
        ok, note = is_real_docx_or_docm(path)
        if ok:
            # note 會是 "DOCX" 或 "DOCM"
            return note, "", (note in ("DOCX", "DOCM"))
        else:
            return "NOT-WORD", f"ZIP 但 {note}", False
    if magic == "ole":
        ok, note = is_ole_word_doc(path)
        if ok:
            return "DOC(legacy)", "", False
        return "OLE-OTHER", note, False
    if magic == "rtf":
        return "RTF", "", False
    if magic == "unknown":
        return "NOT-WORD", "非 ZIP/OLE/RTF 結構", False
    return "UNKNOWN", magic, False

def iter_files(root: Path):
    """scandir 疊代，速度快且能處理權限問題。"""
    stack = [root]
    while stack:
        cur = stack.pop()
        try:
            with os.scandir(cur) as it:
                for e in it:
                    try:
                        if e.is_dir(follow_symlinks=False):
                            stack.append(Path(e.path))
                        elif e.is_file(follow_symlinks=False):
                            yield Path(e.path)
                    except OSError:
                        continue
        except (PermissionError, FileNotFoundError):
            continue

def main():
    ap = argparse.ArgumentParser(
        description="列出資料夾所有檔案並檢查是否為真正的 DOCX（不看副檔名）。"
    )
    ap.add_argument("folder", help="要檢查的資料夾")
    ap.add_argument("--csv", help="輸出 CSV 路徑（UTF-8 BOM）")
    ap.add_argument("--not-docx-only", dest="not_docx_only", action="store_true",
                    help="只輸出『不是 DOCX』的檔案")
    ap.add_argument("--count-docm-as-docx", dest="count_docm_as_docx", action="store_true",
                    help="將 DOCM 視為通過（一起算成 DOCX 類型）")
    args = ap.parse_args()

    root = Path(args.folder).expanduser().resolve()
    if not root.exists() or not root.is_dir():
        raise SystemExit(f"找不到資料夾：{root}")

    rows = []
    stats = {"DOCX":0,"DOCM":0,"DOC(legacy)":0,"RTF":0,"OLE-OTHER":0,"NOT-WORD":0,"UNKNOWN":0}

    for p in iter_files(root):
        kind, note, is_docx_like = classify(p)

        # 若使用者要把 DOCM 視為 DOCX，一起算通過
        is_docx = (kind == "DOCX") or (args.count_docm_as_docx and kind == "DOCM")

        stats[kind] = stats.get(kind, 0) + 1

        if args.not_docx_only and is_docx:
            continue  # 只列出不是 DOCX 的

        rows.append({
            "Path": str(p),
            "Kind": kind,
            "IsDocx": "Y" if is_docx else "N",
            "Ext": p.suffix.lower(),
            "Size": p.stat().st_size if p.exists() else "",
            "Note": note
        })
        print(f"{kind:11s} | {'DOCX' if is_docx else 'NOT'} | {p}")

    print("\n=== 統計 ===")
    for k, v in stats.items():
        print(f"{k:11s}: {v}")

    if args.csv:
        out = Path(args.csv).expanduser().resolve()
        out.parent.mkdir(parents=True, exist_ok=True)
        with open(out, "w", newline="", encoding="utf-8-sig") as f:
            writer = csv.DictWriter(f, fieldnames=["Path","Kind","IsDocx","Ext","Size","Note"])
            writer.writeheader()
            writer.writerows(rows)
        print(f"\n已輸出 CSV：{out}")
    else:
        print("\n（未指定 --csv，僅在主控台列印結果）")

if __name__ == "__main__":
    main()
