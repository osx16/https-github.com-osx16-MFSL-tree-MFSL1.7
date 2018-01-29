﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using MFSL.Models;
using MFSL.ViewModels;
using System.IO;
using PagedList;
using MFSL.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using MFSL.Helpers;

namespace MFSL.Controllers
{
    public class MemberFilesController : Controller
    {

        ApiServices _apiServices = new ApiServices();
        HttpClient client;
        //The URL of the WEB API Service
        string url = "http://localhost:64890/api/MemberFilesAPI";

        public MemberFilesController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Settings.AccessToken);
        }
        // GET: EmployeeInfo
        public async Task<ActionResult> Index()
        {
            HttpResponseMessage responseMessage = await client.GetAsync(url + "/GetFileForUser");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;

                var Employees = JsonConvert.DeserializeObject<List<MemberFile>>(responseData);

                return View(Employees);
            }
            return View("Error");
        }
        public ActionResult RenderInfoView()
        {
            return PartialView("_Info");
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult Recent()
        {
            return View();
        }

        public async Task<ActionResult> MyFiles(string memberNo, int? page)
        {
            int pageSize = 5;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            if (!String.IsNullOrEmpty(memberNo))
            {
                ViewBag.MemberNo = memberNo;
                int id = 0;
                bool isValid = Int32.TryParse(memberNo, out id);
                HttpResponseMessage responseMsg = await client.GetAsync(url + "/GetMyFileByMemberNo/" + id);
                if (responseMsg.IsSuccessStatusCode)
                {
                    var resData = responseMsg.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<List<FileReferences>>(resData);
                    ViewBag.TotalFiles = data.Count;
                    IPagedList<FileReferences> filteredList = (data).ToPagedList(pageIndex, pageSize);
                    return PartialView("_MyFileDetails", filteredList);
                }
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url + "/GetFileForUser");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var fileData = JsonConvert.DeserializeObject<List<FileReferences>>(responseData);
                IPagedList<FileReferences> files = (fileData).ToPagedList(pageIndex, pageSize);
                ViewBag.TotalFiles = fileData.Count;
                return PartialView("_MyFileDetails", files);
            }
            return View("Error");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        // GET: Items
        public async Task<ActionResult> SearchFiles(string memberNo, int? page)
        {
            int pageSize = 5;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            if (!String.IsNullOrEmpty(memberNo))
            {
                ViewBag.MemberNo = memberNo;
                int id = 0;
                bool isValid = Int32.TryParse(memberNo, out id);
                HttpResponseMessage responseMsg = await client.GetAsync(url + "/GetFileByMemberNo/" + id);
                if (responseMsg.IsSuccessStatusCode)
                {
                    var resData = responseMsg.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<List<FileReferences>>(resData);
                    ViewBag.TotalFiles = data.Count;
                    IPagedList<FileReferences> filteredList = (data).ToPagedList(pageIndex, pageSize);
                    return PartialView("_FileDetails", filteredList);
                }
            }

            HttpResponseMessage responseMessage = await client.GetAsync(url + "/GetAll");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var fileData = JsonConvert.DeserializeObject<List<FileReferences>>(responseData);
                ViewBag.TotalFiles = fileData.Count;
                IPagedList<FileReferences> files = (fileData).ToPagedList(pageIndex, pageSize);
                return PartialView("_FileDetails", files);
            }
            return View("Error");
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> GetMemberInfoByNum(string MemberNum)
        {
            int id = 0;
            bool isValid = Int32.TryParse(MemberNum, out id);

            // var member = db.vnpf_.Where(s => s.VNPF_Number == id);
            HttpResponseMessage responseMessage = await client.GetAsync(url + "/GetMemberInfoByNo/" + id);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var fileData = JsonConvert.DeserializeObject<IEnumerable<vnpf_>>(responseData);
                return PartialView("_MemberInfo", fileData);
            }

            return View("Error");
        }

        public ActionResult NewFile()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewFile(
            [Bind(Include = "MemberNo,LoanApplication,OfferLetter," +
            "LoanAgreement,AcceptanceOffer,GuaranteeCertificate," +
            "Amortisation,ChequeCopy,Eligibility,Quotation,Payslip," +
            "LoanStatement,VNPFStatement,Other,FStatusId,")] FileViewModel File)

        {
            if (ModelState.IsValid)
            {
                var LApp = new MemoryStream(); var OL = new MemoryStream(); var LAgrmt = new MemoryStream();
                var AO = new MemoryStream(); var GC = new MemoryStream(); var Amo = new MemoryStream();
                var CC = new MemoryStream(); var Elig = new MemoryStream(); var Quote = new MemoryStream();
                var PS = new MemoryStream(); var LS = new MemoryStream(); var VS = new MemoryStream();
                var Other = new MemoryStream();

                File.LoanApplication.InputStream.CopyTo(LApp); File.OfferLetter.InputStream.CopyTo(OL); File.LoanAgreement.InputStream.CopyTo(LAgrmt);
                File.AcceptanceOffer.InputStream.CopyTo(AO); File.GuaranteeCertificate.InputStream.CopyTo(GC);
                File.Amortisation.InputStream.CopyTo(Amo); File.ChequeCopy.InputStream.CopyTo(CC);
                File.Eligibility.InputStream.CopyTo(Elig); File.Quotation.InputStream.CopyTo(Quote);
                File.Payslip.InputStream.CopyTo(PS); File.LoanStatement.InputStream.CopyTo(LS);
                File.VNPFStatement.InputStream.CopyTo(VS); File.Other.InputStream.CopyTo(Other);
                ViewBag.MemberNo = File.MemberNo;
                var NewMemberFile = new MemberFile
                {
                    DateCreated = System.DateTime.Now,
                    MemberNo = File.MemberNo,
                    LoanApplication = LApp.ToArray(),
                    OfferLetter = OL.ToArray(),
                    LoanAgreement = LAgrmt.ToArray(),
                    AcceptanceOffer = AO.ToArray(),
                    GuaranteeCertificate = GC.ToArray(),
                    Amortisation = Amo.ToArray(),
                    ChequeCopy = CC.ToArray(),
                    Eligibility = Elig.ToArray(),
                    Quotation = Quote.ToArray(),
                    Payslip = PS.ToArray(),
                    LoanStatement = LS.ToArray(),
                    VNPFStatement = VS.ToArray(),
                    Other = Other.ToArray(),
                    FStatusId = 1
                };

                var isSuccess = await _apiServices.CreateNewFile(NewMemberFile);

                if (!isSuccess)
                {
                    ErrorController err = new ErrorController();
                    err.CouldNotCreateFile();
                }
                else
                {
                    var isSuccess2 = await _apiServices.CreateNewRef(NewMemberFile);

                    return RedirectToAction("Recent");
                }
            }

            return View();
        }

        //// GET: MemberFiles/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MemberFile memberFile = await db.MemberFile.FindAsync(id);
        //    if (memberFile == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(memberFile);
        //}

        //// GET: MemberFiles/Create
        //public ActionResult Create()
        //{
        //    ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus");
        //    return View();
        //}

        //// POST: MemberFiles/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "FileNo,DateCreated,OfficeId,MemberNo,LoanApplication,OfferLetter,LoanAgreement,AcceptanceOffer,GuaranteeCertificate,Amortisation,ChequeCopy,Eligibility,Quotation,Payslip,LoanStatement,VNPFStatement,Other,fileGUID,FStatusId")] MemberFile memberFile)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.MemberFile.Add(memberFile);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus", memberFile.FStatusId);
        //    return View(memberFile);
        //}

        //// GET: MemberFiles/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MemberFile memberFile = await db.MemberFile.FindAsync(id);
        //    if (memberFile == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus", memberFile.FStatusId);
        //    return View(memberFile);
        //}

        //// POST: MemberFiles/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "FileNo,DateCreated,OfficeId,MemberNo,LoanApplication,OfferLetter,LoanAgreement,AcceptanceOffer,GuaranteeCertificate,Amortisation,ChequeCopy,Eligibility,Quotation,Payslip,LoanStatement,VNPFStatement,Other,fileGUID,FStatusId")] MemberFile memberFile)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(memberFile).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.FStatusId = new SelectList(db.FileStatus, "FileStatusId", "FStatus", memberFile.FStatusId);
        //    return View(memberFile);
        //}

        //// GET: MemberFiles/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MemberFile memberFile = await db.MemberFile.FindAsync(id);
        //    if (memberFile == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(memberFile);
        //}

        //// POST: MemberFiles/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    MemberFile memberFile = await db.MemberFile.FindAsync(id);
        //    db.MemberFile.Remove(memberFile);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        //[HttpGet]
        //public FileStreamResult DownloadFile(string id, string flag)
        //{
        //    int FileNo = Convert.ToInt32(id);
        //    int fileTypeId = Convert.ToInt32(flag);
        //    string filename = "";
        //    Dictionary<int, string> FileList = new Dictionary<int, string>();
        //    FileList.Add(1, "LoanApplication");
        //    FileList.Add(2, "OfferLetter");
        //    FileList.Add(3, "LoanAgreement");
        //    FileList.Add(4, "AcceptanceOffer");
        //    FileList.Add(5, "GuaranteeCertificate");
        //    FileList.Add(6, "Amortisation");
        //    FileList.Add(7, "ChequeCopy");
        //    FileList.Add(8, "Eligibility");
        //    FileList.Add(9, "Quotation");
        //    FileList.Add(10, "Payslip");
        //    FileList.Add(11, "LoanStatement");
        //    FileList.Add(12, "VNPFStatement");
        //    FileList.Add(13, "Other");

        //    if (FileList.ContainsKey(fileTypeId))
        //    {
        //        filename = FileList[fileTypeId];
        //        Console.WriteLine(filename);
        //    }

        //    string connectionString = @"Data Source=JAS;Initial Catalog=MFSL;Integrated Security=True;
        //                              MultipleActiveResultSets=True;Application Name=EntityFramework";
        //    string commandText = @"SELECT " +filename+ 
        //                        ".PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT() FROM MFSL.dbo.MemberFile WHERE FileNo = @FileNo;";

        //    string serverPath;
        //    byte[] serverTxn;
        //    byte[] buffer = new Byte[1024 * 512];
        //    //byte[] buffer;

        //    using (SqlConnection sqlConnection = new SqlConnection(connectionString))
        //    {

        //        try
        //        {
        //            sqlConnection.Open();

        //            SqlTransaction transaction = sqlConnection.BeginTransaction();
        //            SqlCommand sqlCommand = new SqlCommand();
        //            sqlCommand.Transaction = transaction;
        //            sqlCommand.Connection = sqlConnection;
        //            sqlCommand.CommandText = commandText;
        //            sqlCommand.Parameters.Add("@FileNo", SqlDbType.Int).Value = FileNo;

        //            using (SqlDataReader reader = sqlCommand.ExecuteReader())
        //            {
        //                reader.Read();
        //                serverPath = reader.GetSqlString(0).Value;
        //                serverTxn = reader.GetSqlBinary(1).Value;
        //                reader.Close();
        //            }

        //            using (SqlFileStream sqlFileStream = new SqlFileStream(serverPath, serverTxn, FileAccess.Read))
        //            {
        //                buffer = new Byte[sqlFileStream.Length];
        //                sqlFileStream.Read(buffer, 0, buffer.Length);
        //                sqlFileStream.Close();
        //            }

        //            sqlCommand.Transaction.Commit();

        //        }
        //        catch (System.Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //        }
        //        finally
        //        {
        //            sqlConnection.Close();
        //        }

        //    }//End of SQL Connection Block

        //    return File(new MemoryStream(buffer), "application/pdf");
        //}//End of DownloadFile Method

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}