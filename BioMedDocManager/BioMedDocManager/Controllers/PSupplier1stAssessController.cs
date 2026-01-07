using BioMedDocManager.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioMedDocManager.Controllers
{
    /// <summary>
    /// 初供評核
    /// 定義：於初次供應商評核時，應由請購者就供應品對生產環境、製造流程及產品最終品質等影響品質的潛在風險進行風險分析，將可能的風險因子及影響品質的關聯性進行原因分析紀錄，並依據下方風險等級評估依據判斷供應品項風險等級，並由供應商管理人員進行最終核決確認。
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.Anyone)]
    public class PSupplier1stAssessController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "assess_date";

        /// <summary>
        /// 初次供應商評核表查詢預設排序依據
        /// </summary>
        public const string InitSortDocSearch = "id_no";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "supplier_class", "供應商分類" },
            { "supplier_name", "供應商名稱" },
            { "product_class", "品項編號" },
            { "product_class_title", "品項說明" },
            { "assess_date", "初評日期" },
            { "risk_level", "風險類型" },

        };

        /// <summary>
        /// 初次供應商評核表查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> DocSearchTableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "id_no", "文件編號" },
            { "date_time", "領用日期" },
            { "person_name", "領用人" },
            { "purpose", "領用目的" },
            { "doc_ver", "表單版次" },
        };

        /// <summary>
        /// 顯示初供評核查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<SupplierAssessQueryModel>(SessionKey);

            // 如果query string有帶入page參數，才使用；否則保留Session中的值
            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }

            // 一進來頁面就先按照領用日期倒序
            queryModel.OrderBy ??= InitSort;
            queryModel.SortDir ??= "desc";

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu();

            // 供應商名稱下拉式選單(List)
            ViewData["PurchaseSupplierName"] = SupplierMenu();

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 初供評核查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SupplierAssessQueryModel queryModel)
        {
            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示明細頁面 (供應商資訊+評核結果)
        /// </summary>
        /// <param name="AssessDateString">初評日期</param>
        /// <param name="SupplierName">供應商名稱</param>
        /// <param name="ProductClass">品項編號</param>
        /// <returns></returns>
        [Route("[controller]/Details/{AssessDateString}/{SupplierName}/{ProductClass}")]
        public async Task<IActionResult> Details([FromRoute] string AssessDateString, [FromRoute] string SupplierName, [FromRoute] string ProductClass)
        {
            DateTime AssessDate;
            // 檢查參數是否為null
            if (!DateTime.TryParse(AssessDateString, out AssessDate) || string.IsNullOrWhiteSpace(AssessDateString) || string.IsNullOrWhiteSpace(SupplierName) || string.IsNullOrWhiteSpace(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);

            // 抓取初供資料
            var supplier1stAssess = await context.Supplier1stAssesses
                .Include(s => s.AssessPeopleUser)
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass && m.AssessDate == AssessDate)
                ?? new Supplier1stAssess();

            // 抓合格供應商資料
            var qualifiedSuppliers = await context.QualifiedSuppliers
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass)
                ?? new QualifiedSupplier();

            // 存入合格供應商資料
            ViewData["qualifiedSuppliers"] = qualifiedSuppliers;

            // 0819：先隱藏，因為無採購無需做再評估，但是超過3年沒有採購往來，才需回到初次供應商重新跑流程，所以在初供這邊不用管他有沒有每年評估
            /*string assessStatus = qualifiedSuppliers.Supplier1stAssessDate == null ? "尚未完成" : "已完成";

            
            string reassessReminder;

            if (!qualifiedSuppliers.nextMustAssessmentDate.HasValue)
            {
                reassessReminder = "下次評估日期尚未指定";
            }
            else
            {
                DateTime nextDate = qualifiedSuppliers.nextMustAssessmentDate.Value;

                if (DateTime.Today <= nextDate)
                {
                    reassessReminder = $"下次應於 {nextDate:yyyy/MM/dd} 執行再評估。";
                }
                else
                {
                    reassessReminder = $"惟尚未於 {nextDate:yyyy/MM/dd} 前完成再評估作業，請儘速處理。 ";
                }
            }
            
            ViewData["FirstAssessMessage"] = $"{qualifiedSuppliers.SupplierName}{assessStatus}初次供應商評核，<br>{reassessReminder}";
            ViewData["FirstAssessMessage"] = $"{qualifiedSuppliers.SupplierName}{assessStatus}初次供應商評核";
            */
            return View(supplier1stAssess);
        }

        /// <summary>
        /// 新增供應商
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            // 供應商名稱下拉式選單(List)
            ViewData["PurchaseSupplierName"] = SupplierMenu();
            ViewData["PurchaseSupplierNameDataListIsRequired"] = true;

            // 供應商分類下拉式選單(List)
            ViewData["PurchaseSupplierClassIsRequired"] = true;

            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu(true);
            ViewData["PurchaseProductClassIsRequired"] = true;

            Supplier1stAssess model = new Supplier1stAssess();

            return View(model);
        }

        /// <summary>
        /// 新增供應商儲存
        /// </summary>
        /// <param name="SupplierName">供應商名稱(因為是單獨的input)</param>
        /// <param name="SupplierClass">供應商分類(因為是partial view)</param>
        /// <param name="ProductClass">品項編號(因為是partial view)</param>
        /// <param name="qualifiedSupplier">供應商資料</param>
        /// <param name="supplier1stAssess">初次評核資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] string SupplierName, [FromForm] string SupplierClass, [FromForm] string ProductClass, QualifiedSupplier qualifiedSupplier, [FromForm] Supplier1stAssess supplier1stAssess)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(SupplierName);
                QueryableExtensions.TrimStringProperties(SupplierClass);
                QueryableExtensions.TrimStringProperties(ProductClass);
                QueryableExtensions.TrimStringProperties(qualifiedSupplier);
                QueryableExtensions.TrimStringProperties(supplier1stAssess);

                // 供應商基本資料檢查(因為是用partial view，沒有帶前綴字，所以要改成FromForm)
                if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrEmpty(SupplierClass) || string.IsNullOrEmpty(ProductClass) || supplier1stAssess.AssessDate == null)
                {
                    return DismissModal("初供評核-新增失敗，供應商基本資料有必填項目未填寫!");
                }

                // 初次評核資料
                var supplier1stAssessDB = await context.Supplier1stAssesses
                    .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass && m.AssessDate == supplier1stAssess.AssessDate);
                if (supplier1stAssessDB != null)
                {
                    return DismissModal($"初供評核-新增失敗，該供應商已於【{supplier1stAssess.AssessDate?.ToString("yyyy-MM-dd")}」完成初次評核，不得重複新增!");
                }

                // 供應商是否已存在
                QualifiedSupplier? QualifiedSupplierDB = await context.QualifiedSuppliers.Where(q => q.SupplierName == SupplierName && q.ProductClass == ProductClass).FirstOrDefaultAsync();

                if (QualifiedSupplierDB == null)
                {
                    // 完全全新的供應商
                    var ProductClassesDB = await context.ProductClasses.FirstOrDefaultAsync(m => m.ProductClass1 == ProductClass && !m.ProductClassTitle.Contains("停用"));
                    if (ProductClassesDB == null)
                    {
                        return DismissModal("初供評核-新增失敗，選擇的「品項編號」不存在(或已停用)");
                    }

                    // 供應商補上額外欄位
                    qualifiedSupplier.SupplierName = SupplierName;// 供應商名稱
                    qualifiedSupplier.SupplierClass = SupplierClass;// 供應商分類
                    qualifiedSupplier.ProductClass = ProductClass;// 品項編號
                    qualifiedSupplier.ProductClassTitle = ProductClassesDB.ProductClassTitle;// 品項說明
                    qualifiedSupplier.Supplier1stAssessDate = supplier1stAssess.AssessDate;// 初評日期

                    context.Add(qualifiedSupplier);
                    await context.SaveChangesAsync();
                }

                // 新增初次評核資料
                // 在驗證資料前，補上系統資料
                supplier1stAssess.SupplierName = SupplierName;
                supplier1stAssess.SupplierClass = SupplierClass;
                supplier1stAssess.ProductClass = ProductClass;
                supplier1stAssess.AssessPeople = GetLoginUserId();
                supplier1stAssess.Improvement = supplier1stAssess.AssessResult switch
                {
                    "改善後合格" => supplier1stAssess.Improvement?.Trim(),
                    _ => "N/A" // 或 ""
                };
                supplier1stAssess.Visit = supplier1stAssess.Visit switch
                {
                    "其他" => supplier1stAssess.VisitOther ?? supplier1stAssess.Visit,
                    _ => supplier1stAssess.Visit,
                };

                ModelState.Clear();
                if (!TryValidateModel(supplier1stAssess))
                {
                    return DismissModal("初供評核-編輯失敗，有必填欄位未填寫 或 資料格式不正確或格式錯誤");
                }

                // 新增初供評核紀錄
                context.Supplier1stAssesses.Add(supplier1stAssess);

                await context.SaveChangesAsync();

                // 提交交易
                await transaction.CommitAsync();

                return DismissModal("初供評核-新增成功");

            }
            catch (Exception ex)
            {
                // 回滾交易
                await transaction.RollbackAsync();
                return BadRequest($"錯誤：{ex.Message}");
            }

        }

        /// <summary>
        /// 顯示編輯頁面 (供應商資訊+填單欄位)
        /// </summary>
        /// <param name="AssessDateString">初評日期</param>
        /// <param name="SupplierName">供應商名稱</param>
        /// <param name="ProductClass">品項編號</param>
        /// <returns></returns>
        [Route("[controller]/Edit/{AssessDateString}/{SupplierName}/{ProductClass}")]
        public async Task<IActionResult> Edit([FromRoute] string AssessDateString, [FromRoute] string SupplierName, [FromRoute] string ProductClass)
        {
            DateTime AssessDate;
            // 檢查參數是否為null
            if (!DateTime.TryParse(AssessDateString, out AssessDate) || string.IsNullOrWhiteSpace(AssessDateString) || string.IsNullOrWhiteSpace(SupplierName) || string.IsNullOrWhiteSpace(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);

            // 抓取初供資料
            var supplier1stAssess = await context.Supplier1stAssesses
                .Include(s => s.AssessPeopleUser)
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass && m.AssessDate == AssessDate)
                ?? new Supplier1stAssess();

            // 檢查身分別(若非評核人 且 非自己的初供評核表，不可編輯)
            if (!User.IsInRole(PurchaseRoleStrings.評核人) && supplier1stAssess.AssessPeople != GetLoginUserId())
            {
                return Forbid();
            }

            // 抓合格供應商資料
            var qualifiedSuppliers = await context.QualifiedSuppliers
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass)
                ?? new QualifiedSupplier();

            // 存入合格供應商資料
            ViewData["qualifiedSuppliers"] = qualifiedSuppliers;

            /*
            string assessStatus = qualifiedSuppliers.Supplier1stAssessDate == null ? "尚未完成" : "已完成";
            
            string reassessReminder;

            if (!qualifiedSuppliers.nextMustAssessmentDate.HasValue)
            {
                reassessReminder = "下次評估日期尚未指定";
            }
            else
            {
                DateTime nextDate = qualifiedSuppliers.nextMustAssessmentDate.Value;

                if (DateTime.Today <= nextDate)
                {
                    reassessReminder = $"下次應於 {nextDate:yyyy/MM/dd} 執行再評估。";
                }
                else
                {
                    reassessReminder = $"惟尚未於 {nextDate:yyyy/MM/dd} 前完成再評估作業，請儘速處理。 ";
                }
            }
            
            ViewData["FirstAssessMessage"] = $"{qualifiedSuppliers.SupplierName}{assessStatus}初次供應商評核，<br>{reassessReminder}";
            
            ViewData["FirstAssessMessage"] = $"{qualifiedSuppliers.SupplierName}{assessStatus}初次供應商評核";
            */
            return View(supplier1stAssess);
        }

        /// <summary>
        /// 編輯頁面儲存送出
        /// </summary>
        /// <param name="AssessDateString">初評日期</param>
        /// <param name="SupplierName">供應商名稱</param>
        /// <param name="ProductClass">品項編號</param>
        /// <param name="firstAssessForm">初供評核表資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Edit/{AssessDateString}/{SupplierName}/{ProductClass}")]
        public async Task<IActionResult> Edit([FromRoute] string AssessDateString, [FromRoute] string SupplierName, [FromRoute] string ProductClass, [FromForm] Supplier1stAssess firstAssessForm)
        {
            // 1) 基本參數檢查
            if (string.IsNullOrWhiteSpace(AssessDateString) ||
                string.IsNullOrWhiteSpace(SupplierName) ||
                string.IsNullOrWhiteSpace(ProductClass) ||
                firstAssessForm == null ||
                !DateTime.TryParse(AssessDateString, out var AssessDate))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);
            QueryableExtensions.TrimStringProperties(firstAssessForm);

            // 2) 取得供應商/品項
            var qualifiedSuppliers = await context.QualifiedSuppliers
                .FirstOrDefaultAsync(s => s.SupplierName == SupplierName && s.ProductClass == ProductClass);

            if (qualifiedSuppliers == null)
            {
                return DismissModal("初供評核-編輯失敗，無供應商與品項分類資訊!");
            }

            // 3) 規格化/前置處理（不走自動驗證）
            //    - Visit: 若選「其他」則以 VisitOther 的內容覆蓋；否則清空 VisitOther
            if (string.Equals(firstAssessForm.Visit, "其他", StringComparison.Ordinal))
            {
                firstAssessForm.Visit = (firstAssessForm.VisitOther ?? "其他").Trim();
            }
            else
            {
                firstAssessForm.VisitOther = null;
            }

            //    - Improvement: 僅在「改善後合格」時必填，否則統一為 "N/A"
            if (string.Equals(firstAssessForm.AssessResult, "改善後合格", StringComparison.Ordinal))
            {
                firstAssessForm.Improvement = firstAssessForm.Improvement?.Trim();
            }
            else
            {
                firstAssessForm.Improvement = "N/A";
            }

            // 4) 手動驗證（只驗這 5 欄）
            var errors = new List<string>();

            // 可接受值（若實際選項不同，改這裡就好）
            var validVisits = new HashSet<string>(StringComparer.Ordinal) { "規格符合採購需求", "品質協議(委外製程)", "進行訪視", "其他" };
            var validAssessResults = new HashSet<string>(StringComparer.Ordinal) { "合格", "不合格", "改善後合格" };
            var validRiskLevels = new HashSet<string>(StringComparer.Ordinal) { "高", "中", "低" };

            // 初供評核文件編號 Supplier1stAssessNo
            if (string.IsNullOrWhiteSpace(firstAssessForm.Supplier1stAssessNo))
            {
                errors.Add("初供評核文件編號為必填。");
            }

            // 評估項目 Visit
            if (string.IsNullOrWhiteSpace(firstAssessForm.Visit))
            {
                errors.Add("評估項目為必填。");
            }
            else if (!validVisits.Contains(firstAssessForm.Visit))
            {
                errors.Add($"評估項目僅能為：{string.Join("、", validVisits)}。");
            }

            // 原因 Reason
            if (string.IsNullOrWhiteSpace(firstAssessForm.Reason))
            {
                errors.Add("原因為必填。");
            }

            // 評核結果 AssessResult
            if (string.IsNullOrWhiteSpace(firstAssessForm.AssessResult))
            {
                errors.Add("評核結果為必填。");
            }
            else if (!validAssessResults.Contains(firstAssessForm.AssessResult))
            {
                errors.Add($"評核結果僅能為：{string.Join("、", validAssessResults)}。");
            }

            // 改善狀況 Improvement（僅在「改善後合格」時必填）
            if (string.Equals(firstAssessForm.AssessResult, "改善後合格", StringComparison.Ordinal) &&
                string.IsNullOrWhiteSpace(firstAssessForm.Improvement))
            {
                errors.Add("選擇「改善後合格」時，「改善狀況」為必填。");
            }

            // 風險類型 RiskLevel
            if (string.IsNullOrWhiteSpace(firstAssessForm.RiskLevel))
            {
                errors.Add("風險類型為必填。");
            }
            else if (!validRiskLevels.Contains(firstAssessForm.RiskLevel))
            {
                errors.Add($"風險類型僅能為：{string.Join("、", validRiskLevels)}。");
            }

            // 若有任何手動驗證錯誤，直接回覆，不使用 ModelState/TryValidateModel
            if (errors.Count > 0)
            {
                return DismissModal("初供評核-編輯失敗：\n" + string.Join("\n", errors));
            }

            // 5) 明確設定複合鍵，避免後續 SetValues 嘗試改動鍵值
            firstAssessForm.SupplierName = qualifiedSuppliers.SupplierName;
            firstAssessForm.ProductClass = qualifiedSuppliers.ProductClass;
            firstAssessForm.AssessDate = AssessDate;// 初供的初評日期

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 查既有紀錄（以複合鍵）
                var record = await context.Supplier1stAssesses.FindAsync(
                    qualifiedSuppliers.SupplierName, qualifiedSuppliers.ProductClass, AssessDate);

                if (record != null)
                {
                    // 6) 更新：覆蓋非鍵欄位，並確保鍵欄位不被標示為已修改
                    record.Visit = firstAssessForm.Visit;
                    record.Reason = firstAssessForm.Reason;
                    record.AssessResult = firstAssessForm.AssessResult;
                    record.Improvement = firstAssessForm.Improvement;
                    record.Remarks1 = firstAssessForm.Remarks1;
                    record.RiskLevel = firstAssessForm.RiskLevel;
                    record.Supplier1stAssessNo = firstAssessForm.Supplier1stAssessNo;

                    // 明確確保鍵與不該動的欄位不被修改（保險起見）
                    context.Entry(record).Property(x => x.SupplierName).IsModified = false;
                    context.Entry(record).Property(x => x.ProductClass).IsModified = false;
                    context.Entry(record).Property(x => x.AssessDate).IsModified = false;

                }
                else
                {
                    // *****************************理論上不可能有新增才對*****************************
                    // 7) 新增：已在上面把鍵值寫回 firstAssessForm
                    /*
                    firstAssessForm.AssessPeople = GetLoginUserId();
                    firstAssessForm.SupplierClass = qualifiedSuppliers.SupplierClass;
                    context.Supplier1stAssesses.Add(firstAssessForm);
                    */
                }

                // 8) 若為第一次初審，回填初審日期
                // 備註：會需要回填初審，是因為一開始直接在QualifiedSuppliers新增供應商(自動合格的供應商)，若因為一些特殊狀況，要重新初審時，就會要補初審日期
                qualifiedSuppliers.Supplier1stAssessDate ??= firstAssessForm.AssessDate;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return DismissModal("初供評核-編輯成功!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 初供評核開窗查詢畫面
        /// </summary>
        /// <param name="OrderBy">排序欄位</param>
        /// <param name="SortDir">排序方向</param>
        /// <param name="PageSize">頁面大小</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        public async Task<IActionResult> DocSearchModal([FromQuery] string OrderBy, [FromQuery] string SortDir, [FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {

            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<PurchaseQueryModel>(SessionKey);

            // 若有查詢條件
            queryModel.OrderBy = (!string.IsNullOrEmpty(OrderBy)) ? OrderBy : InitSortDocSearch;
            queryModel.SortDir = (!string.IsNullOrEmpty(SortDir)) ? SortDir : "asc";

            FilterOrderBy(queryModel, DocSearchTableHeaders, InitSortDocSearch);

            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            else if (queryModel.PageSize == 0)
            {
                queryModel.PageSize = 10;
            }

            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }
            else if (queryModel.PageNumber == 0)
            {
                queryModel.PageNumber = 1;
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 固定查詢條件
            var queryParams = new { original_doc_no = "BMP-QP21-TR002" };

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                @$"
                SELECT 
                    dc.id_no, 
                    CONVERT(varchar(10), dc.date_time, 120) AS date_time,
                    u.full_name AS person_name,
                    dc.purpose,
                    dc.doc_ver 
                FROM doc_control_maintable dc
                LEFT JOIN [user] u ON dc.id=u.username
                WHERE dc.original_doc_no = @original_doc_no
                ",
                orderByPart: $"ORDER BY {queryModel.OrderBy} {queryModel.SortDir}",
                pageNumber: 0,// 分頁會影響searchInput搜尋，所以不分頁了
                pageSize: 0,// 分頁會影響searchInput搜尋，所以不分頁了
                parameters: queryParams
            );

            // 即使無資料，也要確認標題存在
            var result = items.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList();

            ViewData["totalCount"] = totalCount;

            ViewData["tableHeaders"] = DocSearchTableHeaders;

            return View(result);

        }

        /// <summary>
        /// AJAX：接收前端傳來的供應商名稱，回傳供應商基本資料 JSON
        /// </summary>
        /// <param name="supplierName">供應商名稱</param>
        /// <returns>供應商查詢結果json</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadQualifiedSuppliers(string? supplierName)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
            {
                return BadRequest(new { message = "供應商名稱不可空白" });
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(supplierName);

            var info = await _context.QualifiedSuppliers
                .Where(s => s.SupplierName == supplierName)
                .Select(s => new
                {
                    supplierNo = s.SupplierNo,
                    supplierClass = s.SupplierClass,
                    tele = s.Tele,
                    productClass = s.ProductClass,
                    tele2 = s.Tele2,
                    remarks = s.Remarks,
                    fax = s.Fax,
                    address = s.Address,
                    supplierInfo = s.SupplierInfo
                })
                .FirstOrDefaultAsync();

            if (info == null)
            {
                return NotFound(new { message = "查無已存檔之供應商資料" });
            }

            return Json(info);
        }

        /*
        ****目前不提供刪除初供評核功能
        /// <summary>
        /// 刪除初供評核表 (撤銷後須重填初供資料) 
        /// </summary>
        /// <param name="firstAssessForm">初供評核表資料</param>
        /// <param name="qualifiedSupplier">供應商資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssess([FromForm] Supplier1stAssess firstAssessForm, [FromForm] QualifiedSupplier qualifiedSupplier)
        {
            using var connection = context.Database.GetDbConnection();
            try
            {
                // Open the connection
                connection.Open();
                // Start the transaction
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    var qualifiedSuppliers = await context.QualifiedSuppliers
                        .FirstOrDefaultAsync(s => s.SupplierName == qualifiedSupplier.SupplierName && s.ProductClass == qualifiedSupplier.ProductClass);

                    if (qualifiedSuppliers == null)
                    {
                        return DismissModal("供應商資訊不存在");
                    }

                    qualifiedSuppliers.Supplier1stAssessDate = null; //刪除評核日期

                    // 刪除評核
                    var record = await context.Supplier1stAssesses.FindAsync(qualifiedSuppliers.SupplierName, qualifiedSuppliers.ProductClass);
                    if (record != null)
                    {
                        context.Supplier1stAssesses.Remove(record);
                    }
                    // Save changes within the transaction
                    await context.SaveChangesAsync();
                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                return DismissModal("評核已刪除!");
            }
            catch (Exception ex)
            {
                // If an exception occurs, rollback the transaction automatically
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
        */

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(SupplierAssessQueryModel queryModel)
        {
            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQuerySupplier1stAssess(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<SupplierAssessQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "初供評核");
            }
            catch (FileNotFoundException)
            {
                //查無結果 不提供檔案
                return NotFound();
            }
        }

        /// <summary>
        /// 載入資料與回傳畫面
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果</returns>
        private async Task<IActionResult> LoadPage(SupplierAssessQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQuerySupplier1stAssess(queryModel, out DynamicParameters parameters, out string sqlDef);
            QueryableExtensions.OrderByFilter(TableHeaders, queryModel, InitSort: InitSort);

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                sqlDef,
                orderByPart: $" ORDER BY {queryModel.OrderBy} {queryModel.SortDir}",
                queryModel.PageNumber,
                queryModel.PageSize,
                parameters
            );

            // 即使無資料，也要確認標題存在
            List<Dictionary<string, object>> result = items?.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList() ?? new List<Dictionary<string, object>>();

            // Pass data to ViewData
            ViewData["totalCount"] = totalCount;
            ViewData["tableHeaders"] = TableHeaders;

            return View(result);
        }

        /// <summary>
        /// 查詢SQL
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <param name="parameters">輸出查詢參數</param>
        /// <param name="sqlQuery">輸出查詢SQL</param>
        private static void BuildQuerySupplier1stAssess(SupplierAssessQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@" 
                SELECT 
                    b.supplier_class, 
                    b.supplier_name,                     
                    b.product_class, 
                    b.product_class_title,
                    a.assess_date,
                    a.risk_level,
                    a.supplier_1st_assess_no,
                    a.assess_people  --有select，但不顯示，是給index table判斷用的
                FROM 
                    supplier_1st_assess a
                LEFT JOIN qualified_suppliers b
                    ON a.supplier_name = b.supplier_name
                   AND a.product_class = b.product_class
                WHERE 1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();


            // 供應商分類
            if (!string.IsNullOrEmpty(queryModel.SupplierClass))
            {
                whereClauses.Add("a.supplier_class LIKE @SupplierClass");
                parameters.Add("SupplierClass", $"%{queryModel.SupplierClass.Trim()}%");
            }

            // 供應商名稱
            if (!string.IsNullOrEmpty(queryModel.SupplierName))
            {
                whereClauses.Add("a.supplier_name LIKE @Supplier");
                parameters.Add("Supplier", $"%{queryModel.SupplierName.Trim()}%");
            }

            // 風險類型
            switch (queryModel.RiskLevel)
            {
                case "低":
                case "中":
                case "高":
                    whereClauses.Add("a.risk_level = @RiskLevel");
                    parameters.Add("RiskLevel", $"{queryModel.RiskLevel.Trim()}");
                    break;
            }

            // 品項分類
            if (!string.IsNullOrEmpty(queryModel.ProductClass))
            {
                whereClauses.Add("a.product_class LIKE @ProductClass");
                parameters.Add("ProductClass", $"%{queryModel.ProductClass.Trim()}%");
            }

            // 初評狀態
            switch (queryModel.HasReviewDate)
            {

                case "已完成評核":
                    whereClauses.Add("b.supplier_1st_assess_date IS NOT NULL");
                    break;
                case "尚未完成評核":
                    whereClauses.Add("b.supplier_1st_assess_date IS NULL");
                    break;
            }


            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }


        }



    }
}
