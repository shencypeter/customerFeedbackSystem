using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CustomerFeedbackSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            // 產生web建立工具
            var builderWeb = WebApplication.CreateBuilder(args);

            // 移除 Kestrel Server Header
            builderWeb.WebHost.ConfigureKestrel(options =>
            {
                options.AddServerHeader = false;
            });

            // 用iis執行要加這行(部屬到正式環境)
            builderWeb.WebHost.UseIISIntegration();

            builderWeb.Services.AddControllersWithViews();
            builderWeb.Services.AddHttpContextAccessor();
            builderWeb.Services.AddDbContext<DocControlContext>(options =>
                options.UseSqlServer(builderWeb.Configuration.GetConnectionString("DefaultConnection")));
            builderWeb.Services.AddDistributedMemoryCache();
            builderWeb.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20); // 20分鐘
            });

            // 全站Cookie Policy：一律加Secure、設定SameSite與HttpOnly
            builderWeb.Services.Configure<CookiePolicyOptions>(o =>
            {
                // 一律要求 Secure（僅 HTTPS 傳輸）
                o.Secure = CookieSecurePolicy.Always;

                // 嚴格 SameSite（防止 CSRF）
                // - Strict：完全禁止跨站送 Cookie（最安全，但可能影響跨站功能）
                // - Lax：允許部分跨站（例如 GET link），但阻擋 POST/iframe
                // 建議：若系統無跨站需求，使用Strict
                o.MinimumSameSitePolicy = SameSiteMode.Strict;

                // 一律 HttpOnly，防止JS讀取Cookie
                o.HttpOnly = HttpOnlyPolicy.Always;
            });

            // 登入驗證
            builderWeb.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Login/Logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);// 20分鐘
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/Error/403"; // 未經授權者，顯示未授權頁面
            });

            // 帳號權限
            builderWeb.Services.AddAuthorization(); // 如果要用 [Authorize] 控制權限

            builderWeb.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(@"C:\DataProtection-Keys"))
            .SetApplicationName("itriDoc");

            // ==============================================
            // 建立web應用程式
            var appWeb = builderWeb.Build();



            // ==============================================
            // 檢查上傳目錄是否存在及可寫入
            // ==============================================

            try
            {
                var uploadRelativePath = builderWeb.Configuration["UploadSettings:UploadPath"];

                if (string.IsNullOrWhiteSpace(uploadRelativePath))
                    throw new InvalidOperationException("UploadSettings:UploadPath is not configured.");

                var uploadFullPath = Path.Combine(
                    appWeb.Environment.ContentRootPath,
                    uploadRelativePath
                );

                // 1. Ensure directory exists
                if (!Directory.Exists(uploadFullPath))
                {
                    Directory.CreateDirectory(uploadFullPath);
                }

                // 2. Write permission test (atomic & disposable)
                var testFile = Path.Combine(uploadFullPath, $".write_test_{Guid.NewGuid():N}.tmp");

                await File.WriteAllTextAsync(testFile, "permission check");
                File.Delete(testFile);

                // Optional: log success
                appWeb.Logger.LogInformation("Upload folder ready: {Path}", uploadFullPath);
            }
            catch (Exception ex)
            {
                // Fail fast — uploads without write access is a broken system
                appWeb.Logger.LogCritical(ex, "Upload folder initialization failed");
                throw;
            }


            // 自訂Headers
            appWeb.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
                // 視環境設定 KnownProxies/KnownNetworks 以避免安全警告
                // KnownProxies = { IPAddress.Parse("127.0.0.1") }
            });

            // 是否為開發環境
            var isDev = appWeb.Environment.IsDevelopment();

            if (!isDev)
            {

                // 正式環境

                // 處理例外
                appWeb.UseExceptionHandler("/Error/500");

                // 處理其他狀態碼錯誤（401, 403, 404）
                appWeb.UseStatusCodePagesWithReExecute("/Error/{0}");

                // 使用HSTS技術
                appWeb.UseHsts();
            }
            else
            {
                // 開發環境：顯示詳細錯誤頁
                appWeb.UseDeveloperExceptionPage();
            }

            //開發環境自動登入 可指定DB存在的帳號
            //UseDevLogin(appWeb, isDev, "petershen");

            // === Clickjacking Protection（全站） ===
            appWeb.Use(async (context, next) =>
            {
                // 1、防止Clickjacking
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";

                // 2、防止 MIME Sniffing
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                // 3、移除其他多餘的 header
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Remove("Server");
                    context.Response.Headers.Remove("X-Powered-By");
                    return Task.CompletedTask;
                });

                await next();
            });


            // Content Security Policy (CSP) 內容安全策略
            appWeb.Use(async (context, next) =>
            {
                // 1、為本次請求產生nonce，並存到Items給View/Controller用
                var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
                context.Items["CspNonce"] = nonce;

                // 2、依環境組出CSP字串
                var csp = new StringBuilder()
                    .Append("default-src 'self'  ;")
                    .Append($"script-src 'self' 'nonce-{nonce}' blob:  ;")
                    .Append("worker-src 'self' blob:  ;")
                    .Append("child-src 'self' blob:  ;") // child-src 已逐漸被 frame-src 取代，兩者都給
                    .Append("frame-src 'self'  ;")        // child-src 已逐漸被 frame-src 取代，兩者都給
                    .Append("style-src 'self'  ;")
                    .Append("img-src 'self' data: blob:  ;")
                    .Append(isDev ? "font-src 'self'  ;" : "font-src 'self' data:  ;")
                    .Append(isDev
                        ? "connect-src 'self' ws: wss: http://localhost:* https://localhost:*  ;"
                        : "connect-src 'self'  ;")
                    .Append("form-action 'self'  ;")      // ✅ 明確允許表單送回本站
                    .Append("object-src 'none'  ;")
                    .Append("base-uri 'self'  ;")
                    .Append("frame-ancestors 'self'  ;");


                if (!isDev)
                {
                    // 正式站可加上這兩條更嚴
                    csp.Append("upgrade-insecure-requests;")
                       .Append("block-all-mixed-content;");
                }

                // 3、在回應要送出前設定Header（確保不被後續覆蓋）
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["Content-Security-Policy"] = csp.ToString();
                    return Task.CompletedTask;
                });

                await next();
            });

            // 自動跳轉HTTP到HTTPS
            appWeb.UseHttpsRedirection();

            // 使用靜態檔案
            appWeb.UseStaticFiles();

            // 使用路由
            appWeb.UseRouting();

            // 指定Cookie策略(一定要在UseSession、UseAuthentication前呼叫)
            appWeb.UseCookiePolicy();

            // 使用Session
            appWeb.UseSession();

            // 使用認證(一定要先認證)
            appWeb.UseAuthentication();

            // 使用授權(授權在後)
            appWeb.UseAuthorization();

            // 使用控制器路由
            appWeb.MapControllerRoute(
                name: "default",
                pattern: "{controller=control}/{action=Index}/{id?}");

            // 執行web應用程式
            appWeb.Run();

        }

        /// <summary>
        /// 開發自動登入 (會無法登出) for 本機zap掃描
        /// </summary>
        /// <param name="appWeb"></param>
        /// <param name="isDev"></param>
        /// <param name="userName"></param>
        private static void UseDevLogin(WebApplication appWeb, bool isDev, string userName)
        {
            if (isDev)
            {
                //為 zap 建議一個永遠登入的狀態 (會無法登出)
                appWeb.Use(async (context, next) =>
                {
                    if (!context.User.Identity?.IsAuthenticated ?? true)
                    {
                        using var scope = context.RequestServices.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<DocControlContext>();

                        // 👤 預定使用者 (無法登出)
                        var username = userName;

                        var user = db.Users
                            .FirstOrDefault(u => u.UserName == username && u.IsActive);

                        if (user != null)
                        {
                            var userRoles = db.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Include(ur => ur.Role)
                                .Select(ur => new { ur.Role.RoleName, ur.Role.RoleGroup })
                                .ToList();

                            var claims = new List<Claim>
                            {
                                new(ClaimTypes.Name, user.UserName),
                                new("FullName", user.FullName),
                                new("UserId", user.Id.ToString())
                            };

                            foreach (var r in userRoles.Select(r => r.RoleName))
                                claims.Add(new Claim(ClaimTypes.Role, r));

                            foreach (var g in userRoles.Select(r => r.RoleGroup).Distinct())
                                claims.Add(new Claim("RoleGroup", g));

                            var identity = new ClaimsIdentity(
                                claims,
                                CookieAuthenticationDefaults.AuthenticationScheme
                            );

                            context.User = new ClaimsPrincipal(identity);
                        }
                    }

                    await next();
                });
            }
        }

        /*
        // 日期格式中介軟體
        public class DateNormalizationMiddleware
        {
            private readonly RequestDelegate _next;

            public DateNormalizationMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            private static readonly string[] DateFormats = new[]
            {
                "yyyy-MM-dd", "yyyy/MM/dd", "MM/dd/yyyy", "dd/MM/yyyy",
                "M/d/yyyy", "d/M/yyyy", "yyyyMMdd"
            };

            public async Task InvokeAsync(HttpContext context)
            {
                if (context.Request.Method == HttpMethods.Post &&
                    context.Request.ContentType != null &&
                    context.Request.ContentType.Contains("application/x-www-form-urlencoded"))
                {
                    context.Request.EnableBuffering();

                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    var parsedForm = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(body);
                    var modifiedForm = new Dictionary<string, StringValues>();

                    foreach (var kv in parsedForm)
                    {
                        if (IsPossiblyDate(kv.Key))
                        {
                            if (TryNormalizeDate(kv.Value, out var normalized))
                            {
                                modifiedForm[kv.Key] = normalized;
                                continue;
                            }
                        }

                        modifiedForm[kv.Key] = kv.Value;
                    }

                    var newForm = new FormUrlEncodedContent(modifiedForm.SelectMany(
                        kvp => kvp.Value.Select(val => new KeyValuePair<string, string>(kvp.Key, val))
                    ));

                    var newBody = await newForm.ReadAsStringAsync();
                    var newBodyBytes = Encoding.UTF8.GetBytes(newBody);

                    context.Request.Body = new MemoryStream(newBodyBytes);
                    context.Request.ContentLength = newBodyBytes.Length;
                }

                await _next(context);
            }

            private bool IsPossiblyDate(string key)
            {
                return key.ToLower().Contains("date");
            }

            private bool TryNormalizeDate(string input, out string normalized)
            {
                normalized = null;
                if (DateTime.TryParseExact(input, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    normalized = parsed.ToString("yyyy-MM-dd"); // normalized for model binder
                    return true;
                }
                return false;
            }
        }
        */
    }
}
