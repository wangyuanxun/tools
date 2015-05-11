using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace Dajau2.Common.Utility
{
    /// <summary>
    /// 图片工具类
    /// </summary>
    public class ImageUtility
    {
        /// <summary>
        /// 图片格式化
        /// </summary>
        private static readonly Dictionary<string, ImageFormat> Formats = new Dictionary<string, ImageFormat>(StringComparer.OrdinalIgnoreCase)
        {
            {".bmp",ImageFormat.Bmp},
            {".emf",ImageFormat.Emf},
            {".exif",ImageFormat.Exif},
            {".gif",ImageFormat.Gif},
            {".icon",ImageFormat.Icon},
            {".jpg",ImageFormat.Jpeg},
            {".jpeg",ImageFormat.Jpeg},
            {".png",ImageFormat.Png},
            {".tiff",ImageFormat.Tiff},
            {".wmf",ImageFormat.Wmf}
        };

        /// <summary> 
        /// 生成缩略图 
        /// </summary> 
        /// <param name="originalImagePath">源图路径（物理路径）</param> 
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param> 
        /// <param name="width">缩略图宽度</param> 
        /// <param name="height">缩略图高度</param> 
        /// <param name="mode">生成缩略图的方式</param>     
        public static void MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode)
        {
            var originalImage = Image.FromFile(originalImagePath, true);

            MakeThumbnail(originalImage, thumbnailPath, width, height, mode);
        }

        /// <summary> 
        /// 生成缩略图 
        /// </summary> 
        /// <param name="originalImage">源图</param> 
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param> 
        /// <param name="width">缩略图宽度</param> 
        /// <param name="height">缩略图高度</param> 
        /// <param name="mode">生成缩略图的方式</param>     
        public static void MakeThumbnail(Image originalImage, string thumbnailPath, int width, int height, string mode)
        {
            var towidth = width;
            var toheight = height;

            var x = 0;
            var y = 0;
            var ow = originalImage.Width;
            var oh = originalImage.Height;

            switch (mode)
            {
                case "HW": //指定高宽缩放（可能变形）                 
                    break;
                case "W": //指定宽，高按比例                     
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H": //指定高，宽按比例 
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut": //指定高宽裁减（不变形）  从左至右截取                   
                    if ((double)originalImage.Width / originalImage.Height > (double)towidth / toheight)
                    {
                        ow = towidth;
                        oh = toheight;
                        y = 0;
                        x = 0;
                    }
                    else
                    {
                        ow = towidth;
                        oh = toheight;
                        x = 0;
                        y = 0;
                    }
                    break;
                case "Cut2": //指定高宽裁减（不变形） 从中间截取                
                    if (originalImage.Width / (double)originalImage.Height > towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
            }

            //新建一个bmp图片 
            Image bitmap = new Bitmap(towidth, toheight);

            //新建一个画板 
            var g = Graphics.FromImage(bitmap);

            //设置高质量插值法 
            g.InterpolationMode = InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充 
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分 
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
                new Rectangle(x, y, ow, oh),
                GraphicsUnit.Pixel);

            try
            {
                //以jpg格式保存缩略图 
                ImageFormat format;
                if (!Formats.TryGetValue(Path.GetExtension(thumbnailPath) ?? string.Empty, out format))
                {
                    format = ImageFormat.Jpeg;
                }
                bitmap.Save(thumbnailPath, format);
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }

        /// <summary> 
        /// 生成缩略图 
        /// </summary> 
        /// <param name="stream">源图</param> 
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param> 
        /// <param name="width">缩略图宽度</param> 
        /// <param name="height">缩略图高度</param> 
        /// <param name="mode">生成缩略图的方式</param>     
        public static void MakeThumbnail(Stream stream, string thumbnailPath, int width, int height, string mode)
        {
            MakeThumbnail(Image.FromStream(stream, true), thumbnailPath, width, height, mode);
        }

        /// <summary>
        /// 生成缩略图
        /// 返回缩略图路径
        /// </summary>
        /// <param name="originalImagePath">原图路径</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）,可以为null，为null时自动生成缩略图路径</param> 
        /// <param name="x">原图选取坐标顶点 左上x</param>
        /// <param name="y">原图选取坐标顶点 左上y</param>
        /// <param name="x2">原图选取坐标顶点 右下x</param>
        /// <param name="y2">原图选取坐标顶点 右下y</param>
        /// <param name="targetWidth">目标图片宽</param>
        /// <param name="targetHeight">目标图片高</param>
        /// <returns>返回缩略图路径</returns>
        public static string MakeThumbnail(string originalImagePath, string thumbnailPath, int x, int y, int x2, int y2, int targetWidth, int targetHeight)
        {
            if (thumbnailPath.IsNullOrWhiteSpace())
            {
                thumbnailPath = GenerateThumbnailPath(originalImagePath, targetWidth, targetHeight);
            }
            using (var originalImage = Image.FromFile(originalImagePath, true))
            {
                if (x < 0)
                {
                    x = 0;
                }
                if (y < 0)
                {
                    y = 0;
                }
                if (x2 <= 0)
                {
                    x2 = originalImage.Width;
                }
                if (y2 <= 0)
                {
                    y2 = originalImage.Height;
                }
                //swap
                if (x > x2)
                {
                    x = x + x2;
                    x2 = x - x2;
                    x = x - x2;
                }
                if (y > y2)
                {
                    y = y + y2;
                    y2 = y - y2;
                    y = y - y2;
                }
                var sourceWidth = x2 - x;
                var sourceHeight = y2 - y;
                var bitmap = new Bitmap(targetWidth, targetHeight);

                //新建一个画板 
                using (var g = Graphics.FromImage(bitmap))
                {
                    //设置高质量插值法 
                    g.InterpolationMode = InterpolationMode.High;

                    //设置高质量,低速度呈现平滑程度 
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    //清空画布并以透明背景色填充 
                    g.Clear(Color.Transparent);

                    g.DrawImage(originalImage, new Rectangle(0, 0, targetWidth, targetHeight),
                        new Rectangle(x, y, sourceWidth, sourceHeight),
                        GraphicsUnit.Pixel);

                    //以jpg格式保存缩略图 
                    var dir = Path.GetDirectoryName(thumbnailPath) ?? string.Empty;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    ImageFormat format;
                    if (!Formats.TryGetValue(Path.GetExtension(originalImagePath) ?? string.Empty, out format))
                    {
                        format = ImageFormat.Jpeg;
                    }
                    bitmap.Save(thumbnailPath, format);
                }
            }
            return thumbnailPath;
        }

        /// <summary>
        /// 根据原图路径生成缩略图路径
        /// 如c:\1.jpg , 100, 100
        /// 返回 c:\100_100\1.jpg
        /// </summary>
        /// <param name="originalImagePath">原图路径</param>
        /// <param name="targetWidth">宽</param>
        /// <param name="targetHeight">高</param>
        /// <returns>
        /// 如c:\1.jpg , 100, 100
        /// 返回 c:\100_100\1.jpg
        /// </returns>
        public static string GenerateThumbnailPath(string originalImagePath, int targetWidth, int targetHeight)
        {
            var dir = Path.GetDirectoryName(originalImagePath) ?? string.Empty;
            var fileName = Path.GetFileName(originalImagePath) ?? string.Empty;

            return Path.Combine(dir, string.Format("{0}_{1}", targetWidth, targetHeight), fileName);
        }

        /// <summary>
        /// 根据原图url生成缩略图url
        /// 如/1.jpg , 100, 100
        /// 返回 /100_100/1.jpg
        /// </summary>
        /// <param name="originalImageUrl">原图url</param>
        /// <param name="targetWidth">宽</param>
        /// <param name="targetHeight">高</param>
        /// <returns></returns>
        public static string GenerateThumbnailUrl(string originalImageUrl, int targetWidth, int targetHeight)
        {
            var baseUrl = originalImageUrl.LastIndexOf("/", StringComparison.Ordinal) > 0
                ? originalImageUrl.Substring(0, originalImageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1)
                : string.Empty;
            var fileName = originalImageUrl.LastIndexOf("/", StringComparison.Ordinal) > 0
                ? originalImageUrl.Substring(originalImageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1)
                : originalImageUrl;
            return baseUrl + string.Format("{0}_{1}", targetWidth, targetHeight) + "/" + fileName;
        }

        #region 正方型裁剪并缩放

        /// <summary>
        /// 正方型裁剪
        /// 以图片中心为轴心，截取正方型，然后等比缩放
        /// 用于头像处理
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="fileSaveUrl">缩略图存放地址</param>
        /// <param name="side">指定的边长（正方型）</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static void CutForSquare(Stream fromFile, string fileSaveUrl, int side, int quality)
        {
            //创建目录
            var dir = Path.GetDirectoryName(fileSaveUrl) ?? string.Empty;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            var initImage = Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= side && initImage.Height <= side)
            {
                initImage.Save(fileSaveUrl, ImageFormat.Jpeg);
            }
            else
            {
                //原始图片的宽、高
                var initWidth = initImage.Width;
                var initHeight = initImage.Height;

                //非正方型先裁剪为正方型
                if (initWidth != initHeight)
                {
                    //截图对象
                    Image pickedImage;
                    Graphics pickedG;

                    //宽大于高的横图
                    if (initWidth > initHeight)
                    {
                        //对象实例化
                        pickedImage = new Bitmap(initHeight, initHeight);
                        pickedG = Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = SmoothingMode.HighQuality;
                        //定位
                        var fromR = new Rectangle((initWidth - initHeight) / 2, 0, initHeight, initHeight);
                        var toR = new Rectangle(0, 0, initHeight, initHeight);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);
                        //重置宽
                        initWidth = initHeight;
                    }
                    //高大于宽的竖图
                    else
                    {
                        //对象实例化
                        pickedImage = new Bitmap(initWidth, initWidth);
                        pickedG = Graphics.FromImage(pickedImage);
                        //设置质量
                        pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        pickedG.SmoothingMode = SmoothingMode.HighQuality;
                        //定位
                        var fromR = new Rectangle(0, (initHeight - initWidth) / 2, initWidth, initWidth);
                        var toR = new Rectangle(0, 0, initWidth, initWidth);
                        //画图
                        pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);
                        //重置高
                        initHeight = initWidth;
                    }

                    //将截图对象赋给原图
                    initImage = (Image)pickedImage.Clone();
                    //释放截图资源
                    pickedG.Dispose();
                    pickedImage.Dispose();
                }

                //缩略图对象
                Image resultImage = new Bitmap(side, side);
                var resultG = Graphics.FromImage(resultImage);
                //设置质量
                resultG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                resultG.SmoothingMode = SmoothingMode.HighQuality;
                //用指定背景色清空画布
                resultG.Clear(Color.White);
                //绘制缩略图
                resultG.DrawImage(initImage, new Rectangle(0, 0, side, side), new Rectangle(0, 0, initWidth, initHeight), GraphicsUnit.Pixel);

                //关键质量控制
                //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                var icis = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo ici = null;
                foreach (var i in icis)
                {
                    if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                    {
                        ici = i;
                    }
                }
                var ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                //保存缩略图
                resultImage.Save(fileSaveUrl, ici, ep);

                //释放关键质量控制所用资源
                ep.Dispose();

                //释放缩略图资源
                resultG.Dispose();
                resultImage.Dispose();

                //释放原始图片资源
                initImage.Dispose();
            }
        }

        #endregion

        #region 自定义裁剪并缩放

        /// <summary>
        /// 指定长宽裁剪
        /// 按模版比例最大范围的裁剪图片并缩放至模版尺寸
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="fileSaveUrl">保存路径</param>
        /// <param name="maxWidth">最大宽(单位:px)</param>
        /// <param name="maxHeight">最大高(单位:px)</param>
        /// <param name="quality">质量（范围0-100）</param>
        public static void CutForCustom(Stream fromFile, string fileSaveUrl, int maxWidth, int maxHeight, int quality)
        {
            //从文件获取原始图片，并使用流中嵌入的颜色管理信息
            var initImage = Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= maxWidth && initImage.Height <= maxHeight)
            {
                initImage.Save(fileSaveUrl, ImageFormat.Jpeg);
            }
            else
            {
                //模版的宽高比例
                var templateRate = 1.0M * maxWidth / maxHeight;
                //原图片的宽高比例
                var initRate = 1.0M * initImage.Width / initImage.Height;

                //原图与模版比例相等，直接缩放
                if (templateRate == initRate)
                {
                    //按模版大小生成最终图片
                    var templateImage = new Bitmap(maxWidth, maxHeight);
                    var templateG = Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = InterpolationMode.High;
                    templateG.SmoothingMode = SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(initImage, new Rectangle(0, 0, maxWidth, maxHeight), new Rectangle(0, 0, initImage.Width, initImage.Height), GraphicsUnit.Pixel);
                    templateImage.Save(fileSaveUrl, ImageFormat.Jpeg);
                }
                //原图与模版比例不等，裁剪后缩放
                else
                {
                    //裁剪对象
                    Image pickedImage;
                    Graphics pickedG;

                    //定位
                    var fromR = new Rectangle(0, 0, 0, 0);//原图裁剪定位
                    var toR = new Rectangle(0, 0, 0, 0);//目标定位

                    //宽为标准进行裁剪
                    if (templateRate > initRate)
                    {
                        //裁剪对象实例化
                        pickedImage = new Bitmap(initImage.Width, (int)Math.Floor(initImage.Width / templateRate));
                        pickedG = Graphics.FromImage(pickedImage);

                        //裁剪源定位
                        fromR.X = 0;
                        fromR.Y = (int)Math.Floor((initImage.Height - initImage.Width / templateRate) / 2);
                        fromR.Width = initImage.Width;
                        fromR.Height = (int)Math.Floor(initImage.Width / templateRate);

                        //裁剪目标定位
                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = initImage.Width;
                        toR.Height = (int)Math.Floor(initImage.Width / templateRate);
                    }
                    //高为标准进行裁剪
                    else
                    {
                        pickedImage = new Bitmap((int)Math.Floor(initImage.Height * templateRate), initImage.Height);
                        pickedG = Graphics.FromImage(pickedImage);

                        fromR.X = (int)Math.Floor((initImage.Width - initImage.Height * templateRate) / 2);
                        fromR.Y = 0;
                        fromR.Width = (int)Math.Floor(initImage.Height * templateRate);
                        fromR.Height = initImage.Height;

                        toR.X = 0;
                        toR.Y = 0;
                        toR.Width = (int)Math.Floor(initImage.Height * templateRate);
                        toR.Height = initImage.Height;
                    }

                    //设置质量
                    pickedG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    pickedG.SmoothingMode = SmoothingMode.HighQuality;

                    //裁剪
                    pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);

                    //按模版大小生成最终图片
                    Image templateImage = new Bitmap(maxWidth, maxHeight);
                    var templateG = Graphics.FromImage(templateImage);
                    templateG.InterpolationMode = InterpolationMode.High;
                    templateG.SmoothingMode = SmoothingMode.HighQuality;
                    templateG.Clear(Color.White);
                    templateG.DrawImage(pickedImage, new Rectangle(0, 0, maxWidth, maxHeight), new Rectangle(0, 0, pickedImage.Width, pickedImage.Height), GraphicsUnit.Pixel);

                    //关键质量控制
                    //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
                    var icis = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo ici = null;
                    foreach (var i in icis)
                    {
                        if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                        {
                            ici = i;
                        }
                    }
                    var ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                    //保存缩略图
                    templateImage.Save(fileSaveUrl, ici, ep);
                    //templateImage.Save(fileSaveUrl, Imaging.ImageFormat.Jpeg);

                    //释放资源
                    templateG.Dispose();
                    templateImage.Dispose();

                    pickedG.Dispose();
                    pickedImage.Dispose();
                }
            }

            //释放资源
            initImage.Dispose();
        }
        #endregion

        #region 等比缩放

        /// <summary>
        /// 图片等比缩放
        /// </summary>
        /// <remarks>吴剑 2012-08-08</remarks>
        /// <param name="fromFile">原图Stream对象</param>
        /// <param name="savePath">缩略图存放地址</param>
        /// <param name="targetWidth">指定的最大宽度</param>
        /// <param name="targetHeight">指定的最大高度</param>
        /// <param name="watermarkText">水印文字(为""表示不使用水印)</param>
        /// <param name="watermarkImage">水印图片路径(为""表示不使用水印)</param>
        public static void ZoomAuto(Stream fromFile, string savePath, double targetWidth, double targetHeight, string watermarkText, string watermarkImage)
        {
            //创建目录
            string dir = Path.GetDirectoryName(savePath) ?? string.Empty;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //原始图片（获取原始图片创建对象，并使用流中嵌入的颜色管理信息）
            var initImage = Image.FromStream(fromFile, true);

            //原图宽高均小于模版，不作处理，直接保存
            if (initImage.Width <= targetWidth && initImage.Height <= targetHeight)
            {
                //文字水印
                if (watermarkText != "")
                {
                    using (Graphics gWater = Graphics.FromImage(initImage))
                    {
                        var fontWater = new Font("黑体", 10);
                        Brush brushWater = new SolidBrush(Color.White);
                        gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                        gWater.Dispose();
                    }
                }

                //透明图片水印
                if (watermarkImage != "")
                {
                    if (File.Exists(watermarkImage))
                    {
                        //获取水印图片
                        using (Image wrImage = Image.FromFile(watermarkImage))
                        {
                            //水印绘制条件：原始图片宽高均大于或等于水印图片
                            if (initImage.Width >= wrImage.Width && initImage.Height >= wrImage.Height)
                            {
                                Graphics gWater = Graphics.FromImage(initImage);

                                //透明属性
                                var imgAttributes = new ImageAttributes();
                                var colorMap = new ColorMap
                                {
                                    OldColor = Color.FromArgb(255, 0, 255, 0),
                                    NewColor = Color.FromArgb(0, 0, 0, 0)
                                };
                                ColorMap[] remapTable = { colorMap };
                                imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                                float[][] colorMatrixElements = { 
                                   new [] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                   new [] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                   new [] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                   new [] {0.0f,  0.0f,  0.0f,  0.5f, 0.0f},//透明度:0.5
                                   new [] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                };

                                var wmColorMatrix = new ColorMatrix(colorMatrixElements);
                                imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                gWater.DrawImage(wrImage, new Rectangle(initImage.Width - wrImage.Width, initImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);

                                gWater.Dispose();
                            }
                            wrImage.Dispose();
                        }
                    }
                }

                //保存
                initImage.Save(savePath, ImageFormat.Jpeg);
            }
            else
            {
                //缩略图宽、高计算
                double newWidth = initImage.Width;
                double newHeight = initImage.Height;

                //宽大于高或宽等于高（横图或正方）
                if (initImage.Width > initImage.Height || initImage.Width == initImage.Height)
                {
                    //如果宽大于模版
                    if (initImage.Width > targetWidth)
                    {
                        //宽按模版，高按比例缩放
                        newWidth = targetWidth;
                        newHeight = initImage.Height * (targetWidth / initImage.Width);
                    }
                }
                //高大于宽（竖图）
                else
                {
                    //如果高大于模版
                    if (initImage.Height > targetHeight)
                    {
                        //高按模版，宽按比例缩放
                        newHeight = targetHeight;
                        newWidth = initImage.Width * (targetHeight / initImage.Height);
                    }
                }

                //生成新图
                //新建一个bmp图片
                Image newImage = new Bitmap((int)newWidth, (int)newHeight);
                //新建一个画板
                var newG = Graphics.FromImage(newImage);

                //设置质量
                newG.InterpolationMode = InterpolationMode.HighQualityBicubic;
                newG.SmoothingMode = SmoothingMode.HighQuality;

                //置背景色
                newG.Clear(Color.White);
                //画图
                newG.DrawImage(initImage, new Rectangle(0, 0, newImage.Width, newImage.Height), new Rectangle(0, 0, initImage.Width, initImage.Height), GraphicsUnit.Pixel);

                //文字水印
                if (watermarkText != "")
                {
                    using (Graphics gWater = Graphics.FromImage(newImage))
                    {
                        var fontWater = new Font("宋体", 10);
                        Brush brushWater = new SolidBrush(Color.White);
                        gWater.DrawString(watermarkText, fontWater, brushWater, 10, 10);
                        gWater.Dispose();
                    }
                }

                //透明图片水印
                if (watermarkImage != "")
                {
                    if (File.Exists(watermarkImage))
                    {
                        //获取水印图片
                        using (var wrImage = Image.FromFile(watermarkImage))
                        {
                            //水印绘制条件：原始图片宽高均大于或等于水印图片
                            if (newImage.Width >= wrImage.Width && newImage.Height >= wrImage.Height)
                            {
                                var gWater = Graphics.FromImage(newImage);

                                //透明属性
                                var imgAttributes = new ImageAttributes();
                                var colorMap = new ColorMap
                                {
                                    OldColor = Color.FromArgb(255, 0, 255, 0),
                                    NewColor = Color.FromArgb(0, 0, 0, 0)
                                };
                                ColorMap[] remapTable = { colorMap };
                                imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                                float[][] colorMatrixElements = { 
                                   new [] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                   new [] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                   new[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                   new[] {0.0f,  0.0f,  0.0f,  0.5f, 0.0f},//透明度:0.5
                                   new [] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                };

                                var wmColorMatrix = new ColorMatrix(colorMatrixElements);
                                imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                gWater.DrawImage(wrImage, new Rectangle(newImage.Width - wrImage.Width, newImage.Height - wrImage.Height, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);
                                gWater.Dispose();
                            }
                            wrImage.Dispose();
                        }
                    }
                }

                //保存缩略图
                newImage.Save(savePath, ImageFormat.Jpeg);

                //释放资源
                newG.Dispose();
                newImage.Dispose();
                initImage.Dispose();
            }
        }

        #endregion

        #region 其它

        /// <summary>
        /// 判断文件类型是否为WEB格式图片
        /// (注：JPG,GIF,BMP,PNG)
        /// </summary>
        /// <param name="contentType">HttpPostedFile.ContentType</param>
        /// <returns></returns>
        public static bool IsWebImage(string contentType)
        {
            if (contentType == "image/pjpeg" || contentType == "image/jpeg" || contentType == "image/gif" || contentType == "image/bmp" || contentType == "image/png")
            {
                return true;
            }
            return false;
        }

        #endregion

        #region 获取图片URL
        /// <summary>
        /// 取得HTML中首个图片的 URL。
        /// </summary>
        /// <param name="sHtmlText">HTML代码</param>
        /// <returns>图片的URL列表</returns>
        public static string GetHtmlImageUrlList(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串
            MatchCollection matches = regImg.Matches(sHtmlText);

            int i = 0;
            //string[] sUrlList = new string[matches.Count];

            //// 取得匹配项列表
            //foreach (Match match in matches)
            //    sUrlList[i++] = match.Groups["imgUrl"].Value;

            //return sUrlList;

            if (matches.Count > 0)
                return matches[0].Groups["imgUrl"].Value;
            else
                return "";
        }
        #endregion
    }
}
