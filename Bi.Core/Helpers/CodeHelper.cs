﻿using Bi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;
using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Bi.Core.Helpers
{
    using ZXing.ZKWeb;

    /// <summary>
    /// 二维码/条形码/验证码工具类
    /// </summary>
    public class CodeHelper
    {
        #region 创建二维码图片
        #region CreateQr
        /// <summary>
        /// 创建二维码图片
        /// </summary>
        /// <param name="contents">要生成二维码包含的信息</param>
        /// <param name="qrPath">生成的二维码路径</param>
        /// <param name="width">二维码宽度</param>
        /// <param name="height">二维码高度</param>
        /// <param name="options">二维码配置</param>
        /// <returns>bool</returns>
        public static bool CreateQr(string contents, string qrPath, int width = 300, int height = 300, QrCodeEncodingOptions options = null)
        {
            if (contents.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(contents)}”不能是 Null 或为空", nameof(contents));

            if (qrPath.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(qrPath)}”不能是 Null 或为空", nameof(qrPath));

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options ?? new QrCodeEncodingOptions
                {
                    DisableECI = true,
                    CharacterSet = "UTF-8",
                    Width = width,
                    Height = height,
                    ErrorCorrection = ErrorCorrectionLevel.H,
                }
            };

            using var bitmap = writer.Write(contents);
            if (File.Exists(qrPath))
                File.Delete(qrPath);
            bitmap.Save(qrPath);
            return true;
        }

        /// <summary>
        /// 创建中间带有logo的二维码图片
        /// </summary>
        /// <param name="contents">要生成二维码包含的信息</param>
        /// <param name="logoPath">二维码中间的logo图片路径</param>
        /// <param name="qrPath">生成的二维码路径</param>
        /// <param name="width">二维码宽度</param>
        /// <param name="height">二维码高度</param>
        /// <returns>bool</returns>
        public static bool CreateQr(string contents, string logoPath, string qrPath, int width = 300, int height = 300)
        {
            if (contents.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(contents)}”不能是 Null 或为空", nameof(contents));

            if (qrPath.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(qrPath)}”不能是 Null 或为空", nameof(qrPath));

            if (logoPath.IsNullOrEmpty())
                return CreateQr(contents, qrPath, width, height);

            using var logoImage = Image.FromFile(logoPath);

            //构造二维码写码器
            var mutiWriter = new MultiFormatWriter();
            var hint = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.CHARACTER_SET] = "UTF-8",
                [EncodeHintType.ERROR_CORRECTION] = ErrorCorrectionLevel.H
            };

            //生成二维码
            var bm = mutiWriter.encode(contents, BarcodeFormat.QR_CODE, width, height, hint);
            var barcodeWriter = new BarcodeWriter();
            using var bitmap = barcodeWriter.Write(bm);

            //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
            var rectangle = bm.getEnclosingRectangle();

            //计算插入图片的大小和位置
            var middleImgW = Math.Min((int)(rectangle[2] / 3.5), logoImage.Width);
            var middleImgH = Math.Min((int)(rectangle[3] / 3.5), logoImage.Height);
            var middleImgL = (bitmap.Width - middleImgW) / 2;
            var middleImgT = (bitmap.Height - middleImgH) / 2;

            //将img转换成bmp格式，否则后面无法创建 Graphics对象
            using var bmpimg = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(bitmap, 0, 0);
            }

            //在二维码中插入图片
            using (var myGraphic = Graphics.FromImage(bmpimg))
            {
                //白底
                myGraphic.FillRectangle(Brushes.White, middleImgL, middleImgT, middleImgW, middleImgH);
                myGraphic.DrawImage(logoImage, middleImgL, middleImgT, middleImgW, middleImgH);
            }

            if (File.Exists(qrPath))
                File.Delete(qrPath);
            bmpimg.Save(qrPath);

            return true;
        }
        #endregion

        #region CreateQrBase64
        /// <summary>
        /// 创建二维码图片，返回base64字符串
        /// </summary>
        /// <param name="contents">要生成二维码包含的信息</param>
        /// <param name="width">二维码宽度</param>
        /// <param name="height">二维码高度</param>
        /// <param name="imageFormat">图片格式，默认：png</param>
        /// <param name="options">二维码配置</param>
        /// <returns>bool</returns>
        public static string CreateQrBase64(string contents, int width = 300, int height = 300, ImageFormat imageFormat = null, QrCodeEncodingOptions options = null)
        {
            if (contents.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(contents)}”不能是 Null 或为空", nameof(contents));

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options ?? new QrCodeEncodingOptions
                {
                    DisableECI = true,
                    CharacterSet = "UTF-8",
                    Width = width,
                    Height = height,
                    ErrorCorrection = ErrorCorrectionLevel.H,
                }
            };

            using var bitmap = writer.Write(contents);
            using var ms = new MemoryStream();
            bitmap.Save(ms, imageFormat ?? ImageFormat.Png);
            return $"data:image/{(imageFormat == ImageFormat.Jpeg ? "jpeg" : "png")};base64,{Convert.ToBase64String(ms.ToArray())}";
        }

        /// <summary>
        /// 创建中间带有logo的二维码图片，返回base64字符串
        /// </summary>
        /// <param name="contents">要生成二维码包含的信息</param>
        /// <param name="logoPath">二维码中间的logo图片路径</param>
        /// <param name="width">二维码宽度</param>
        /// <param name="height">二维码高度</param>
        /// <param name="imageFormat">图片格式，默认：png</param>
        /// <returns>bool</returns>
        public static string CreateQrBase64(string contents, string logoPath, int width = 300, int height = 300, ImageFormat imageFormat = null)
        {
            if (contents.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(contents)}”不能是 Null 或为空", nameof(contents));

            if (logoPath.IsNullOrEmpty())
                return CreateQrBase64(contents, width, height, imageFormat);

            using var logoImage = Image.FromFile(logoPath);

            //构造二维码写码器
            var mutiWriter = new MultiFormatWriter();
            var hint = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.CHARACTER_SET] = "UTF-8",
                [EncodeHintType.ERROR_CORRECTION] = ErrorCorrectionLevel.H
            };

            //生成二维码
            var bm = mutiWriter.encode(contents, BarcodeFormat.QR_CODE, width, height, hint);
            var barcodeWriter = new BarcodeWriter();
            using var bitmap = barcodeWriter.Write(bm);

            //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
            var rectangle = bm.getEnclosingRectangle();

            //计算插入图片的大小和位置
            var middleImgW = Math.Min((int)(rectangle[2] / 3.5), logoImage.Width);
            var middleImgH = Math.Min((int)(rectangle[3] / 3.5), logoImage.Height);
            var middleImgL = (bitmap.Width - middleImgW) / 2;
            var middleImgT = (bitmap.Height - middleImgH) / 2;

            //将img转换成bmp格式，否则后面无法创建 Graphics对象
            using var bmpimg = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(bitmap, 0, 0);
            }

            //在二维码中插入图片
            using (var myGraphic = Graphics.FromImage(bmpimg))
            {
                //白底
                myGraphic.FillRectangle(Brushes.White, middleImgL, middleImgT, middleImgW, middleImgH);
                myGraphic.DrawImage(logoImage, middleImgL, middleImgT, middleImgW, middleImgH);
            }

            using var ms = new MemoryStream();
            bmpimg.Save(ms, imageFormat ?? ImageFormat.Png);
            return $"data:image/{(imageFormat == ImageFormat.Jpeg ? "jpeg" : "png")};base64,{Convert.ToBase64String(ms.ToArray())}";
        }
        #endregion
        #endregion

        #region 创建条形码图片
        #region CreateBar
        /// <summary>
        /// 创建条形码
        /// </summary>
        /// <param name="contents">要生成条形码包含的信息</param>
        /// <param name="barPath">生成的条形码路径</param>
        /// <param name="width">条形码宽度</param>
        /// <param name="height">条形码高度</param>
        /// <param name="options">条形码配置</param>
        /// <returns>bool</returns>
        public static bool CreateBar(string contents, string barPath, int width = 150, int height = 50, EncodingOptions options = null)
        {
            if (contents.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(contents)}”不能是 Null 或为空", nameof(contents));

            if (barPath.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(barPath)}”不能是 Null 或为空", nameof(barPath));

            var writer = new BarcodeWriter
            {
                //使用ITF 格式，不能被现在常用的支付宝、微信扫出来
                //如果想生成可识别的可以使用 CODE_128 格式
                //writer.Format = BarcodeFormat.ITF;
                Format = BarcodeFormat.CODE_128,
                Options = options ?? new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 2
                }
            };

            using var bitmap = writer.Write(contents);
            if (File.Exists(barPath))
                File.Delete(barPath);
            bitmap.Save(barPath);
            return true;
        }
        #endregion

        #region CreateBarBase64
        /// <summary>
        /// 创建条形码，返回base64字符串
        /// </summary>
        /// <param name="contents">要生成条形码包含的信息</param>
        /// <param name="width">条形码宽度</param>
        /// <param name="height">条形码高度</param>
        /// <param name="imageFormat">图片格式，默认：png</param>
        /// <param name="options">条形码配置</param>
        /// <returns>bool</returns>
        public static string CreateBarBase64(string contents, int width = 150, int height = 50, ImageFormat imageFormat = null, EncodingOptions options = null)
        {
            if (contents.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(contents)}”不能是 Null 或为空", nameof(contents));

            var writer = new BarcodeWriter
            {
                //使用ITF 格式，不能被现在常用的支付宝、微信扫出来
                //如果想生成可识别的可以使用 CODE_128 格式
                //writer.Format = BarcodeFormat.ITF;
                Format = BarcodeFormat.CODE_128,
                Options = options ?? new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 2
                }
            };

            using var bitmap = writer.Write(contents);
            using var ms = new MemoryStream();
            bitmap.Save(ms, imageFormat ?? ImageFormat.Png);
            return $"data:image/{(imageFormat == ImageFormat.Jpeg ? "jpeg" : "png")};base64,{Convert.ToBase64String(ms.ToArray())}";
        }
        #endregion
        #endregion

        #region 读取二维码或者条形码
        /// <summary>
        /// 读取二维码或者条形码内容
        /// </summary>
        /// <param name="path">二维码/条形码图片路径</param>
        /// <returns>string</returns>
        public static string ReadCode(string path)
        {
            if (path.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(path)}”不能是 Null 或为空", nameof(path));

            if (!File.Exists(path))
                return null;

            var reader = new BarcodeReader();
            reader.Options.CharacterSet = "UTF-8";
            using var bitmap = new Bitmap(path);
            var result = reader.Decode(bitmap);

            return result?.Text;
        }
        #endregion

        #region 验证码
        /// <summary>
        /// 生成验证码字节数组
        /// </summary>
        /// <param name="validateCode">验证码字符串</param>
        /// <returns>byte</returns>
        public static byte[] CreateValidateGraphic(string validateCode)
        {
            if (validateCode.IsNullOrEmpty())
                throw new ArgumentException($"“{nameof(validateCode)}”不能是 Null 或为空", nameof(validateCode));

            using var image = new Bitmap((int)Math.Ceiling(validateCode.Length * 16.0), 27);
            using var g = Graphics.FromImage(image);

            //生成随机生成器
            var random = new Random();

            //清空图片背景色
            g.Clear(Color.White);

            //画图片的干扰线
            for (var i = 0; i < 25; i++)
            {
                var x1 = random.Next(image.Width);
                var x2 = random.Next(image.Width);
                var y1 = random.Next(image.Height);
                var y2 = random.Next(image.Height);
                g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
            }

            using (var font = new Font("Arial", 13, (FontStyle.Bold | FontStyle.Italic)))
            {
                using var brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 2);
            }

            //画图片的前景干扰点
            for (var i = 0; i < 100; i++)
            {
                var x = random.Next(image.Width);
                var y = random.Next(image.Height);
                image.SetPixel(x, y, Color.FromArgb(random.Next()));
            }

            //画图片的边框线
            g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

            //保存图片数据
            using var stream = new MemoryStream();

            image.Save(stream, ImageFormat.Jpeg);

            //输出图片流
            return stream.ToArray();
        }
        #endregion
    }
}
