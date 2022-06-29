using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RSCMSWeb.Models;
using context1 = System.Web.HttpContext;
namespace RSCMSWeb.App_Start
{

    public class RSCMSActionFilter : ActionFilterAttribute
    {


        private ApplicationDbContext context = new ApplicationDbContext();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //  Log("OnActionExecuting", filterContext.RouteData);

            //check for authorisation

            var currentUser = filterContext.HttpContext.User.Identity.GetUserId();
            string CMSBaseUrl = ConfigurationManager.AppSettings["CMSBaseUrl"];
            //if (currentUser != null)
            //{
            //    var username = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUser).UserName;
            //}
            var controllerName = filterContext.RouteData.Values["controller"].ToString();
            var actionName = filterContext.RouteData.Values["action"].ToString();
            var menuId = context.Menu.Where(m => m.RouteAction.Equals(actionName)).Select(t => t.Id).FirstOrDefault();

           // var isAuthorised = context.UserMenu.Where(um => um.UserId.Equals(currentUser) && um.MenuId.Equals(menuId)).Any();
            var isAuthorised = context.UserMenu.Where(um => um.MenuId.Equals(menuId)).Any();
            //if ajax request
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                if (isAuthorised)
                {
                    return;
                }
            }


            var breadCrumb = new Dictionary<string, string>();



            // if(filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")

            //if (!filterContext.HttpContext.Request.IsAjaxRequest())
            //{
            //    var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            //    var role = userManager.GetRoles(currentUser).FirstOrDefault();
            //    if (!role.Contains("SuperAdminUser")) {
            //        filterContext.Result = new RedirectResult("/Account/login");
            //        return;
            //    }
            //}
            //else
            // {
            if ((actionName != "login") && (actionName != "LogOff"))
            {
                if ((actionName != "Index") && (controllerName != "Home"))
                {
                    if (!isAuthorised)
                    {
                        filterContext.Result = new RedirectResult("/Home/Index");
                        return;
                    }
                }


                //get all menus for this user            
                var menu = (from m in context.Menu
                            join um in context.UserMenu on m.Id equals um.MenuId
                            where um.UserId.Equals(currentUser)// && m.Sequence !=0
                            select m).Distinct().ToList();
                //new { Id = m.Id, Depth = m.Depth, Name = m.Name, ParentId = m.ParentId, Route = m.Route }).ToList();
                var viewBag = filterContext.Controller.ViewBag;
            
                viewBag.contextMenu = menu;
                if (actionName == "login")
                {
                    breadCrumb.Clear();
                }
                else if (actionName == "Index" && controllerName == "Home")
                {
                    breadCrumb.Add("Home", "/Home/Index");
                }
                else
                {
                    breadCrumb.Add("Home", "/Home/Index");
                        var iteml = context.Menu.Where(t => t.RouteController.Contains(controllerName) && t.RouteAction.Contains(actionName));

                    if (iteml != null || iteml.Count() > 0)
                    {
                        var item = iteml.FirstOrDefault();
                        if (item.Depth == 1)
                            //  breadCrumb.Add("" + item.Name, "/" + item.RouteController + "/" + item.RouteAction);
                            breadCrumb.Add("" + item.Name, "#");

                        if (item.Depth == 2)
                        {
                            try
                            {
                                var parent = menu.Where(t => t.Id.Equals(item.ParentId)).FirstOrDefault();
                            }
                            catch
                            {

                            }
                            //breadCrumb.Add("" + parent.Name, "/" + parent.RouteController + "/" + parent.RouteAction);
                            // breadCrumb.Add("" + parent.Name, "/" + "Home" + "/" + "Index");
                            //breadCrumb.Add("" + parent.Name, "#");

                            if (actionName.Contains("Index"))
                            {
                                //breadCrumb.Add("" + item.Name, "/" + item.RouteController + "/" + item.RouteAction);
                                if (HttpContext.Current.Request.QueryString["MenuId"] != null)
                                {
                                    var menuItem = menu.Where(t => t.Id == Convert.ToInt32(HttpContext.Current.Request.QueryString["MenuId"])).FirstOrDefault();
                                    if (menuItem != null)
                                        breadCrumb.Add("" + menuItem.Name, "#");
                                    else
                                        breadCrumb.Add("" + item.Name, "#");
                                }
                                else
                                    breadCrumb.Add("" + item.Name, "#");

                            }
                            else
                            {
                                //var routestr = actionName.Split('_');
                                var indexAction = context.Menu.Where(t => t.RouteController.Equals(controllerName) && t.RouteAction.Contains("Index")).FirstOrDefault();
                                if (controllerName == "LarrdisOrientation")
                                {
                                    if (actionName.Contains("LarrdisOrientation"))
                                    {
                                        var RouteAction = "LarrdisOrientation_Index";
                                        var Name = "Orientation Program (Information Booklet Series)";
                                        breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                        breadCrumb.Add("" + item.Name, "#");
                                    }

                                    else if (actionName.Contains("LarrdisPractice"))
                                    {
                                        var RouteAction = "LarrdisPractice_Index";
                                        var Name = "PRACTICE AND PROCEDURE SERIES";
                                        breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                        breadCrumb.Add("" + item.Name, "#");
                                    }
                                    else if (actionName.Contains("LarrdisPetitions"))
                                    {
                                        var RouteAction = "LarrdisPetitions_Index";
                                        var Name = "Petitions-Procedure for submission";
                                        breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                        breadCrumb.Add("" + item.Name, "#");
                                    }
                                    else if (actionName.Contains("LarrdisRulings_Observations"))
                                    {
                                        var RouteAction = "LarrdisRulings_Observations_Index";
                                        var Name = "Rulings and Observations from the Chair";
                                        breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                        breadCrumb.Add("" + item.Name, "#");
                                    }
                                }
                                else if (controllerName == "Adminstration" && actionName.Contains("WebSection"))
                                {
                                    var RouteAction = "WebSection_Index";
                                    var Name = "Web Section Master";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Adminstration" && actionName.Contains("SG"))
                                {
                                    var RouteAction = "SG_Index";
                                    var Name = "SG";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "BillOffice" && actionName.Contains("BillsSection"))
                                {
                                    var RouteAction = "BillsSection_Index";
                                    var Name = "Bills Section";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "LobbySessionJournals" && actionName.Contains("MainPage"))
                                {
                                    var RouteAction = "MainPage_Index";
                                    var Name = " Main Page of journals";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "TablePaperLaidInRajyaSabha" && actionName.Contains("ReportType"))
                                {
                                    var RouteAction = "ReportType_Index";
                                    var Name = "Report Type";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "BillOffice" && actionName.Contains("LatestBills"))
                                {
                                    var RouteAction = "LatestBills_Index";
                                    var Name = "Latest Bills";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Procedure" && actionName.Contains("PracticeAndProcedure"))
                                {
                                    var RouteAction = "PracticeAndProcedure_Index";
                                    var Name = "Practice & Procedure - Abstract Series";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Procedure" && actionName.Contains("GovernmentInstructions"))
                                {
                                    var RouteAction = "GovernmentInstructions_Index";
                                    var Name = "Government Instructions On Dealing with MPs";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Procedure" && actionName.Contains("PrivilegeDigest"))
                                {
                                    var RouteAction = "PrivilegeDigest_Index";
                                    var Name = "Privilege Digest";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Procedure" && actionName.Contains("RulingsAndObservation"))
                                {
                                    var RouteAction = "RulingsAndObservation_Index";
                                    var Name = "Rulings and Observation";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Procedure" && actionName.Contains("RajyaSabhaAtWork"))
                                {
                                    var RouteAction = "RajyaSabhaAtWork_Index";
                                    var Name = "RajyaSabha at Work";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Debates" && actionName.Contains("VerbatimDebates"))
                                {
                                    var RouteAction = "VerbatimDebates_Index";
                                    var Name = "Verbatim Debates";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if (controllerName == "Debates" && actionName.Contains("Discussions"))
                                {
                                    var RouteAction = "Discussions_Index";
                                    var Name = "Discussions";
                                    breadCrumb.Add("" + Name, "/" + indexAction.RouteController + "/" + RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                                else if((controllerName != "Committee" || !actionName.Contains("Introduction"))&& (controllerName != "Procedure" || !actionName.Contains("ProcedureForSubmission")))
                                {
                                    breadCrumb.Add("" + indexAction.Name, "/" + indexAction.RouteController + "/" + indexAction.RouteAction);
                                    // breadCrumb.Add("" + indexAction.Name, "#");
                                    // breadCrumb.Add("" + item.Name, "/" + item.RouteController + "/" + item.RouteAction);
                                    breadCrumb.Add("" + item.Name, "#");
                                }
                            }
                        }
                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("/Home/Index");
                        return;
                    }

                }
                foreach (var item in breadCrumb.ToList())
                {
                    if (!string.IsNullOrEmpty(item.Value) && item.Value != "#")
                        breadCrumb[item.Key] = CMSBaseUrl + breadCrumb[item.Key];

                }
                viewBag.BreadCrumb = breadCrumb;
            }

            Logger.LogtoFile(string.Empty);
            //add logger
            try
            {

                //get url
                var url = context1.Current.Request.Url.ToString();
                //ip
                //var ip = context1.Current.Request.UserHostAddress;
                String ip =
        HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ip))
                {
                    ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                //browser name

                HttpRequest req = context1.Current.Request;
                var browserName = req.Browser.Browser;
                //get user name
                var username = context1.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(currentUser).UserName;
                //get Error log stacktrace,


                //text file path
                string filepath = context1.Current.Server.MapPath("~/Logger/");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                //text file name
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();

                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("-----------Logger Details on " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine("User Id:  " + "" + currentUser);
                    sw.WriteLine("User:   " + "" + username);
                    sw.WriteLine("Url:   " + "" + url);
                    sw.WriteLine("IP:   " + "" + ip);
                    sw.WriteLine("Browser Name:   " + "" + browserName);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                e.ToString();

            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Log("OnActionExecuted", filterContext.RouteData);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // Log("OnResultExecuting", filterContext.RouteData);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // Log("OnResultExecuted", filterContext.RouteData);
        }

        private void Log(string methodName, RouteData routeData)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var message = string.Format("{0} controller: {1} action: {2}", methodName, controllerName, actionName);
            Debug.WriteLine(message, "Action Filter Log");
        }
    }
}

