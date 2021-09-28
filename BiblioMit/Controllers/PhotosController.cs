using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BiblioMit.Data;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Services;
//using Amazon.S3;
//using Amazon.S3.Model;
using BiblioMit.Models.ViewModels;
using Microsoft.AspNetCore.Localization;
//using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Histopathology;
using System.Net;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class PhotosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhotosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Gallery()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetImg(string f, string d)
        {
            var name = Regex.Replace(f, ".*/", "");

            var full = Path.Combine(Directory.GetCurrentDirectory(),
                                    d, name);

            return PhysicalFile(full, "image/jpg");
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        // GET: Photos/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photos
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        // GET: Photos/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["IndividualId"] = _context.Individuals.GroupBy(i => i.SamplingId);
            var mags = from Magnification e in Enum.GetValues(typeof(Magnification))
                        select new { Id = e, Name = e.GetAttrName() };
            ViewData["Magnification"] = new SelectList(mags, "Id", "Name");

            return View();
        }

        // POST: Photos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //public async Task<IActionResult> Create([Bind("IndividualId,File,Comment,Magnification")] UploadPhotoViewModel uploadPhoto)
        //{
        //    if (uploadPhoto == null) return NotFound();
        //    //var client = new AmazonS3Client(_accessKey, _secretKey, Amazon.RegionEndpoint.SAEast1);

        //    //var contentDisposition = ContentDispositionHeaderValue.Parse(uploadPhoto.File.ContentDisposition);

        //    //var filename = contentDisposition.FileName.Trim('"');

        //    //var stream = uploadPhoto.File.OpenReadStream();
        //    var file = uploadPhoto.File.FileName;
        //    if (string.IsNullOrWhiteSpace(file) || file.IndexOfAny(InvalidFilenameChars) >= 0 || Path.GetFileName(file) != file)
        //        return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        //    var path = Path.Combine(Directory.GetCurrentDirectory(),
        //                "DB", uploadPhoto.File.FileName);
        //    //using var stream = new StreamReader(path);
        //    //var content = await stream.ReadToEndAsync().ConfigureAwait(false);
        //    //var request = new PutObjectRequest
        //    //{
        //    //    BucketName = _bucket,
        //    //    Key = uploadPhoto.IndividualId.ToString()+"/"+filename,
        //    //    InputStream = stream,
        //    //    CannedACL = S3CannedACL.Private
        //    //};

        //    //var response = await client.PutObjectAsync(request);

        //    var photo = new Photo
        //    {
        //        IndividualId = uploadPhoto.IndividualId,
        //        Key = uploadPhoto.File.FileName,
        //        Comment = uploadPhoto.Comment,
        //        Magnification = uploadPhoto.Magnification
        //    };

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(photo);
        //        await _context.SaveChangesAsync().ConfigureAwait(false);
        //        return RedirectToAction("Index");
        //    }
        //    return View(photo);
        //}

        //public IActionResult Tmp()
        //{
        //    //var client = new AmazonS3Client(_accessKey, _secretKey, Amazon.RegionEndpoint.SAEast1);

        //    var photos = _context.Photo.Include(p => p.Individual).Where(p => p.Individual.SamplingId > 953).Skip(6);

        //    foreach (var photo in photos)
        //    {
        //        var file = Regex.Replace(photo.Key, @"^.*/", "");

        //        //var request = new PutObjectRequest
        //        //{
        //        //    BucketName = _bucket,
        //        //    Key = photo.Key,
        //        //    FilePath = "NewDB/" + file,
        //        //    CannedACL = S3CannedACL.Private
        //        //};

        //        //var response = await client.PutObjectAsync(request);
        //    }

        //    return RedirectToAction("Index");
        //}

        // GET: Photos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photos.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (photo == null)
            {
                return NotFound();
            }
            ViewData["IndividualId"] = new SelectList(_context.Set<Individual>(), "Id", "Id", photo.IndividualId);
            return View(photo);
        }

        // POST: Photos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IndividualId,Comment,Magnification,Key")] Photo photo)
        {
            if (photo == null || id != photo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(photo);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhotoExists(photo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["IndividualId"] = new SelectList(_context.Set<Individual>(), "Id", "Id", photo.IndividualId);
            return View(photo);
        }

        // GET: Photos/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photos
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        // POST: Photos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var photo = await _context.Photos.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        private bool PhotoExists(int id)
        {
            return _context.Photos.Any(e => e.Id == id);
        }
    }
    public class NanoGalleryElement
    {
        public string Src { get; set; }
        public string Srct { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(EmitDefaultValue = false)]
        public string AlbumId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(EmitDefaultValue = false)]
        public string Kind { get; set; }
    }
}
