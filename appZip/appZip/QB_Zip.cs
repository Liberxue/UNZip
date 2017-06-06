﻿using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;



namespace Installation
{

    // <summary>
    /// 压缩类
    /// </summary>
    public class ZipClass
    {
         /// <summary>
         /// 递归压缩文件夹方法
         /// </summary>
         /// <param name="FolderToZip"></param>
         /// <param name="s"></param>
         /// <param name="ParentFolderName"></param>
         private   bool ZipFileDictory(string FolderToZip, ZipOutputStream s, string ParentFolderName)
          {
             bool res = true;
             string[] folders, filenames;
             ZipEntry entry = null;
             FileStream fs = null;
             Crc32 crc = new Crc32();
             try{
                 //创建当前文件夹
                 entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/")); //加上 “/” 才会当成是文件夹创建
                 s.PutNextEntry(entry);
                 s.Flush();
                //先压缩文件，再递归压缩文件夹
                 filenames = Directory.GetFiles(FolderToZip);
                 foreach (string file in filenames){
                     //打开压缩文件
                     fs = File.OpenRead(file);
                     byte[] buffer = new byte[fs.Length];
                     fs.Read(buffer, 0, buffer.Length);
                     entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/" + Path.GetFileName(file)));
                     entry.DateTime = DateTime.Now;
                     entry.Size = fs.Length;
                     fs.Close();
                     crc.Reset();
                     crc.Update(buffer);
                     entry.Crc = crc.Value;
                     s.PutNextEntry(entry);
                     s.Write(buffer, 0, buffer.Length);
                 }
             }catch{
                res = false;
             }finally{
                 if (fs != null)
                 {
                    fs.Close();
                    fs = null;
                 }
                 if (entry != null)
                 {
                  entry = null;
                 }
                 GC.Collect();
                 GC.Collect(1);
             }

             folders = Directory.GetDirectories(FolderToZip);

             foreach (string folder in folders){
                if (!ZipFileDictory(folder, s, Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip)))){
                    return false;
                }
             }
             return res;
         }

         /// <summary>
         /// 压缩目录
         /// </summary>
         /// <param name="FolderToZip">待压缩的文件夹，全路径格式</param>
         /// <param name="ZipedFile">压缩后的文件名，全路径格式</param>
         /// <param name="Password">压宿密码</param>
         /// <returns></returns>
         private bool ZipFileDictory(string FolderToZip, string ZipedFile, String Password){
            bool res;
            if (!Directory.Exists(FolderToZip)){
               return false;
            }
            ZipOutputStream s = new ZipOutputStream(File.Create(ZipedFile));
            s.SetLevel(6);
            s.Password = Password;
            res = ZipFileDictory(FolderToZip, s, "");
            s.Finish();
            s.Close();
            return res;
         }

        
         /// <summary>
         /// 压缩多个目录或文件
         /// </summary>
         /// <param name="FolderToZip">待压缩的文件夹，全路径格式</param>
         /// <param name="ZipedFile">压缩后的文件名，全路径格式</param>
         /// <param name="Password">压宿密码</param>
         /// <returns></returns>
         private bool ZipManyFilesDictorys(string FolderToZip, string ZipedFile, String Password) {
             //多个文件分开
             string[] filsOrDirs = FolderToZip.Split('%');
             bool res = true ;
             ZipOutputStream s = new ZipOutputStream(File.Create(ZipedFile));
             s.SetLevel(6);
             s.Password = Password;
             foreach (string fileOrDir in filsOrDirs){
                 //是文件夹
                 if (Directory.Exists(fileOrDir))
                 {
                     res = ZipFileDictory(fileOrDir, s, "");
                 }
                 else  //文件
                 {
                     res = ZipFileWithStream(fileOrDir, s, "");
                 }
             }
             s.Finish();
             s.Close();
             return res;
         }

        
         /// <summary>
         /// 带压缩流压缩单个文件
         /// </summary>
         /// <param name="FileToZip">要进行压缩的文件名</param>
         /// <param name="ZipedFile">压缩后生成的压缩文件名</param>
         /// <param name="Password">压宿密码</param>
         /// <returns></returns>
         private bool ZipFileWithStream(string FileToZip, ZipOutputStream ZipStream, String Password)
         {
             //如果文件没有找到，则报错
             if (!File.Exists(FileToZip))
             {
                 throw new System.IO.FileNotFoundException("指定要压缩的文件: " + FileToZip + " 不存在!");
             }
             //FileStream fs = null;
             FileStream ZipFile = null;
             ZipEntry ZipEntry = null;
             bool res = true;
             try
             {
                 ZipFile = File.OpenRead(FileToZip);
                 byte[] buffer = new byte[ZipFile.Length];
                 ZipFile.Read(buffer, 0, buffer.Length);
                 ZipFile.Close();
                 ZipEntry = new ZipEntry(Path.GetFileName(FileToZip));
                 ZipStream.PutNextEntry(ZipEntry);
                 ZipStream.Write(buffer, 0, buffer.Length);
             }
             catch
             {
                 res = false;
             }
             finally
             {
                 if (ZipEntry != null)
                 {
                     ZipEntry = null;
                 }
               
                 if (ZipFile != null)
                 {
                     ZipFile.Close();
                     ZipFile = null;
                 }
                 GC.Collect();
                 GC.Collect(1);
             }
             return res;
           
         }

         /// <summary>
         /// 压缩文件
         /// </summary>
         /// <param name="FileToZip">要进行压缩的文件名</param>
         /// <param name="ZipedFile">压缩后生成的压缩文件名</param>
         /// <param name="Password">压宿密码</param>
         /// <returns></returns>
         private  bool ZipFile(string FileToZip, string ZipedFile, String Password){

            //如果文件没有找到，则报错
             if (!File.Exists(FileToZip)){
                 throw new System.IO.FileNotFoundException("指定要压缩的文件: " + FileToZip + " 不存在!");
            }
            //FileStream fs = null;
             FileStream ZipFile = null;
             ZipOutputStream ZipStream = null;
             ZipEntry ZipEntry = null;
             bool res = true;
             try{
                 ZipFile = File.OpenRead(FileToZip);
                 byte[] buffer = new byte[ZipFile.Length];
                 ZipFile.Read(buffer, 0, buffer.Length);
                 ZipFile.Close();
                 ZipFile = File.Create(ZipedFile);
                 ZipStream = new ZipOutputStream(ZipFile);
                 ZipStream.Password = Password;
                 ZipEntry = new ZipEntry(Path.GetFileName(FileToZip));
                 ZipStream.PutNextEntry(ZipEntry);
                 ZipStream.SetLevel(6);
                 ZipStream.Write(buffer, 0, buffer.Length);
             }catch {
                res = false;
             }finally{
                if (ZipEntry != null){
                    ZipEntry = null;
                 }
                 if (ZipStream != null){
                     ZipStream.Finish();
                     ZipStream.Close();
                 }
                 if (ZipFile != null){
                     ZipFile.Close();
                     ZipFile = null;
                 }
                 GC.Collect();
                 GC.Collect(1);
            }
            return res;
        }

 

        /// <summary>
        /// 压缩文件 和 文件夹
        /// </summary>
        /// <param name="FileToZip">待压缩的文件或文件夹，全路径格式多个用%号分隔</param>
        /// <param name="ZipedFile">压缩后生成的压缩文件名，全路径格式</param>
        /// <returns></returns>
        public  bool Zip(String FileToZip, String ZipedFile, String Password){

            if (IsFilesOrFolders(FileToZip)){
                return ZipManyFilesDictorys(FileToZip, ZipedFile, Password);
            }else if (Directory.Exists(FileToZip)){  //单个文件夹
                return ZipFileDictory(FileToZip, ZipedFile, Password);
            }else if (File.Exists(FileToZip)){  //单个文件
                return ZipFile(FileToZip, ZipedFile, Password);
            }else{
               return false;
            }
         }

        //是否传入的多个文件夹或文件或两者都有
        private bool IsFilesOrFolders(string fileFolders){
           
            if (fileFolders.Split('%').Length > 1) {
                return true;
            }
            else{
                return false;
            }
        }
     }


    /// <summary>
    /// 解压类=======================================
    /// </summary>
    public class UnZipClass
    {
        /// <summary>
        /// 解压功能(解压压缩文件到指定目录)
        /// </summary>
        /// <param name="FileToUpZip">待解压的文件</param>
        /// <param name="ZipedFolder">指定解压目标目录</param>
        /// <param name="Password">解压密码</param>
        public bool UnZip(string FileToUpZip, string ZipedFolder,string Password){
           
            if (!File.Exists(FileToUpZip)){
                return false;
            }
            if (!Directory.Exists(ZipedFolder)){
                Directory.CreateDirectory(ZipedFolder);
            }
            ZipInputStream s = null;
            ZipEntry theEntry = null;
            string fileName;
            FileStream streamWriter = null;
            try{
                s = new ZipInputStream(File.OpenRead(FileToUpZip));
           
                s.Password = Password;
                while ((theEntry = s.GetNextEntry()) != null){
                    if (theEntry.Name != String.Empty){
                        fileName = Path.Combine(ZipedFolder, theEntry.Name);
                        ///判断文件路径是否是文件夹
                        if (fileName.EndsWith("/") || fileName.EndsWith("\\")){
                            Directory.CreateDirectory(fileName);
                            continue;
                        }
                        streamWriter = File.Create(fileName);
                        int size = 2048;
                        byte[] data = new byte[2048];

                        while (true){
                            size = s.Read(data, 0, data.Length);
                            if (size > 0){
                                streamWriter.Write(data, 0, size);
                            }
                            else{
                                break;
                            }
                        }
                    }
                }

                return true;
            }catch{
                //Console.WriteLine(ex.Message);
                return false;
            }finally{
                if (streamWriter != null){
                    streamWriter.Close();
                    streamWriter = null;
                }
                if (theEntry != null){
                    theEntry = null;
                }
                if (s != null){
                    s.Close();
                    s = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
        }

      
    }

}


