using FileManagerCore.Models;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagerCore.Controllers
{
    public class FileManagerCoreController : Controller
    {


        private readonly string[] arrImages = new string[] { "jpg", "jpeg", "png", "gif", "bmp", "tiff" };
        private readonly string[] arrFiles = new string[] { "doc", "docx", "pdf", "xls", "xlsx", "txt", "csv", "html", "psd", "sql", "log", "fla", "xml", "ade", "adp", "ppt", "pptx", "zip", "rar" };
        private readonly string[] arrVideos = new string[] { "mov", "mpeg", "mp4", "avi", "mpg", "wma" };
        private readonly string[] arrMusic = new string[] { "mp3", "m4a", "ac3", "aiff", "mid" };
        private readonly string[] arrMisc = new string[] { "mp3", "m4a", "ac3", "aiff", "mid" };
        private readonly string uploadPath = "";
        private readonly string thumbPath = ".thumbs";
        private readonly bool allowUploadFile = true;
        private readonly bool allowDeleteFile = true;
        private readonly bool allowCreateFolder = true;
        private readonly bool allowDeleteFolder = true;
        private readonly int maxUploadSizeMB = 100;
        private readonly string rootPath = "";
        private readonly string rootUrl = "";
        private readonly string fillSelector = "";
        private readonly string popupCloseCode = "";




        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetInformation(string path)
        {
            var items = new List<FMCItemModelView>();


            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadPath, path?.TrimStart('/')?.TrimStart('\\') ?? string.Empty);
            var rootThumbPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", thumbPath, path?.TrimStart('/')?.TrimStart('\\') ?? string.Empty);

            //load folders
            var arrFolders = Directory.GetDirectories(rootPath)?.Where(x=> !x.Contains(".thumbs"));
            foreach (string folder in arrFolders)
            {
                var item = new FMCItemModelView()
                {
                    IsFolder = true,
                    ImageThumbs = "../imgs/folder.png",
                    Name = Path.GetFileName(folder),
                    Path = path
                };
                items.Add(item);
            }

            // load files
            var arrFiles = Directory.GetFiles(rootPath);
            foreach (var file in arrFiles)
            {
                FileInfo info = new FileInfo(file);
                var item = new FMCItemModelView()
                {
                    Name = Path.GetFileName(file),
                    IsImage = isImageFile(file) == true,
                    MimeType = MimeTypes.GetMimeType(file),
                    Size = FormatSize(info.Length),
                    FullImage = ConvertPathServerToPathWeb(file),
                    ImageThumbs = isImageFile(file)? ConvertPathServerToPathWeb(createThumbnail(file, rootThumbPath + "/" + Path.GetFileName(file))) :GetImageFile(file),
                    Path = System.IO.Path.GetDirectoryName(ConvertPathServerToPathWeb(file,false))
                };
                items.Add(item);
            }
            return Json(items);

        }


        [HttpPost]
        public IActionResult CreateFolder(string folder)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadPath, folder?.TrimStart('/')?.TrimStart('\\') ?? string.Empty);
            var rootThumbPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", thumbPath, folder?.TrimStart('/')?.TrimStart('\\') ?? string.Empty);

            Directory.CreateDirectory(rootPath);
            Directory.CreateDirectory(rootPath);
            return Ok();
        }

        [HttpPost]
        public IActionResult Delete(string path,bool isfile)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadPath, path?.TrimStart('/')?.TrimStart('\\') ?? string.Empty);
            var rootThumbPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", thumbPath, path?.TrimStart('/')?.TrimStart('\\') ?? string.Empty);

            if (isfile)
            {
                System.IO.File.Delete(rootPath);
                //System.IO.File.Delete(rootThumbPath);
            }
                
            else
                System.IO.Directory.Delete(rootPath,true);
            return Ok();
        }

        [HttpPost]

        public async Task<IActionResult> UploadFile(string folder, IFormFile file)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadPath, folder?.TrimStart('/')?.TrimStart('\\') ?? string.Empty);
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(rootPath, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return Ok();
        }


        #region Functions ConvertPath,GetImageFile,FormatSize

        private string ConvertPathServerToPathWeb(string file,bool withdot=true)
        {
            var index = file.ToLower().IndexOf("wwwroot");
            if (index > -1)
                return (withdot?"..":String.Empty) + file.Substring(index + "wwwroot".Length, file.Length - (index + "wwwroot".Length)).Replace("\\","/");
            return file;
        }

        private string GetImageFile(string file)
        {
            var defaultPath = "imgs";

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", defaultPath);

            var extention = Path.GetExtension(file).Trim('.');

            var temp = Path.Combine(rootPath, extention+".png");
            var temp2 = Path.Combine(rootPath, extention.TrimEnd('x')+".png");


            if (System.IO.File.Exists(temp))
                return ConvertPathServerToPathWeb(temp);
            if (System.IO.File.Exists(temp2))
                return ConvertPathServerToPathWeb(temp2);

            temp = Path.Combine(rootPath, "any.png");

            return ConvertPathServerToPathWeb(temp);
        }

        static readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        public static string FormatSize(Int64 bytes)
        {
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        #endregion

        #region Function is Type File

        private bool isImageFile(string strFilename)
        {
            int intPosition;

            intPosition = Array.IndexOf(arrImages, Path.GetExtension(strFilename).ToLower().TrimStart('.'));
            return (intPosition > -1);  // if > -1, then it was found in the list of image file extensions
        } // isImageFile

        private bool isVideoFile(string strFilename)
        {
            int intPosition;

            intPosition = Array.IndexOf(arrVideos, Path.GetExtension(strFilename).ToLower().TrimStart('.'));
            return (intPosition > -1);  // if > -1, then it was found in the list of video file extensions
        } // isVideoFile

        private bool isMusicFile(string strFilename)
        {
            int intPosition;

            intPosition = Array.IndexOf(arrMusic, Path.GetExtension(strFilename).ToLower().TrimStart('.'));
            return (intPosition > -1);  // if > -1, then it was found in the list of music file extensions
        } // isMusicFile

        private bool isMiscFile(string strFilename)
        {
            int intPosition;

            intPosition = Array.IndexOf(arrMisc, Path.GetExtension(strFilename).ToLower().TrimStart('.'));
            return (intPosition > -1);  // if > -1, then it was found in the list of misc file extensions
        } // isMiscFile

        #endregion

        #region thumbnail Images

        private string createThumbnail(string strFilename, string strThumbFilename)
        {
            try
            {


                var directory = Path.GetDirectoryName(strThumbFilename);
                if(!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                var file = new FileInfo(strFilename);

                using (MagickImage image = new MagickImage(file))
                {
                    {
                        image.Thumbnail(new MagickGeometry(286));
                        image.Write(strThumbFilename);
                    }
                }
                return strThumbFilename;
            }
            catch
            {
                return strFilename;
            }
        } 

        private void getProportionalResize(int intOldWidth, int intOldHeight, ref int intNewWidth, ref int intNewHeight)
        {
            int intHDiff = 0;
            int intWDiff = 0;
            decimal decProp = 0;
            int intTargH = 78;
            int intTargW = 156;

            if ((intOldHeight <= intTargH) && (intOldWidth <= intTargW))
            {
                // no resize needed
                intNewHeight = intOldHeight;
                intNewWidth = intOldWidth;
                return;
            }

            //get the differences between desired and current height and width
            intHDiff = intOldHeight - intTargH;
            intWDiff = intOldWidth - intTargW;

            //whichever is the bigger difference is the chosen proportion
            if (intHDiff > intWDiff)
            {
                decProp = (decimal)intTargH / (decimal)intOldHeight;
                intNewHeight = intTargH;
                intNewWidth = Convert.ToInt32(Math.Round(intOldWidth * decProp, 0));
            }
            else
            {
                decProp = (decimal)intTargW / (decimal)intOldWidth;
                intNewWidth = intTargW;
                intNewHeight = Convert.ToInt32(Math.Round(intOldHeight * decProp, 0));
            }
        } // getProportionalResize


        #endregion 


    }
}
