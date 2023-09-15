using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;
using System.IO;
using System.Diagnostics;

namespace videoApp
{
    public class Editor
    {
        private int fps;
        private int frameWidth;
        private int frameHeight;
        private int totalFrames;


        public void ProcessVideoImport(string videoPath, string outputVideoPath, int newWidth, int newHeight)
        {
            using var videoCapture = new VideoCapture(videoPath);
            var fps = videoCapture.Fps;
            var frameWidth = videoCapture.FrameWidth;
            var frameHeight = videoCapture.FrameHeight;
            var totalFrames = videoCapture.FrameCount;

            var targetDuration = 60; // 60 giây
            var  targetFrameCount = targetDuration * fps;

            // Tạo video writer để xuất video cắt và thay đổi kích thước ra file mới
            using (var videoWriter = new VideoWriter(outputVideoPath, FourCC.H264, fps, new OpenCvSharp.Size(newWidth, newHeight), true))
            {
                int width = 720;
                int height = 720;
                Mat blackImage = new Mat(height, width, MatType.CV_8UC3, new Scalar(0, 0, 0));

                for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
                {
                    using (var frame = new Mat())
                    {
                        videoCapture.Read(frame);
                        if (frame == null || frame.Empty())
                            break;

                        // Cắt video thành 60 giây
                        if (frameIndex >= targetFrameCount)
                            break;

                        // Thay đổi kích thước của frame
                        using var resizedFrame = new Mat();
                        Cv2.Resize(frame, resizedFrame, new OpenCvSharp.Size(newWidth, newHeight - 213), 0, 0, InterpolationFlags.Linear);

                        //// Thêm phần màu đen vào video
                        int newHeightWithBoard = (int)(newHeight * 1.2);

                        // Tạo một khung màu đen với cùng kích thước với frame
                        using var blackBoardFrame = new Mat();
                        Cv2.Resize(blackImage, blackBoardFrame, new OpenCvSharp.Size(newWidth, newHeight), 0, 0, InterpolationFlags.Linear);
                        // Đặt frame gốc vào giữa khung màu đen
                        var roi = new Rect(0, (int)(newHeight - resizedFrame.Height), newWidth, newHeight-213);
                        resizedFrame.CopyTo(blackBoardFrame[roi]);
                        // Ghi khung màu đen vào video mới
                        videoWriter.Write(blackBoardFrame);
                        //Giải phóng tài nguyên
                        blackBoardFrame.Dispose();
                        resizedFrame.Dispose();
                        frame.Dispose();
                    }
                }
            }

            videoCapture.Dispose();
        }
        public string[] GetImageFilesFromDirectory(string directoryPath)
        {
            // Lấy danh sách tất cả các tệp hình ảnh (jpg, png, bmp, gif)
            var imageFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => file.ToLower().EndsWith("jpg") || file.ToLower().EndsWith("png") || file.ToLower().EndsWith("bmp") || file.ToLower().EndsWith("gif"))
                .OrderBy(file => file) // Sắp xếp theo tên tệp để đảm bảo chúng được thêm vào video theo thứ tự
                .ToArray();

            return imageFiles;
        }
        public string[] GetAudioFilesFromDirectory(string audioDirectoryPath)
        {
           
            // Lấy danh sách tất cả các tệp âm thanh (mp3, wav)
            var audioFiles = Directory.GetFiles(audioDirectoryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => file.ToLower().EndsWith("mp3") || file.ToLower().EndsWith("wav"))
                .OrderBy(file => file) // Sắp xếp theo tên tệp để đảm bảo chúng được thêm vào video theo thứ tự
                .ToArray();

            return audioFiles;
        }
        public async Task InsertAudioEveryIntervalAsync(string inputVideoPath, string audioFolderPath, string outputVideoPath, double interval)
        {
            // Lấy danh sách tất cả các tệp âm thanh trong thư mục
            string[] audioFiles = Directory.GetFiles(audioFolderPath, "*.mp3");

            // Sắp xếp danh sách tệp âm thanh theo thứ tự tên tệp (nếu không sắp xếp, thời điểm bắt đầu có thể không đồng đều)
            Array.Sort(audioFiles);

            // Khởi tạo đối tượng ProcessStartInfo để gọi FFmpeg
            ProcessStartInfo ffmpegStartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Users\thich\ffmpeg-2023-07-19-git-efa6cec759-full_build\bin\ffmpeg.exe", // Đường dẫn đến tệp thực thi FFmpeg (nếu đã được thêm vào biến môi trường PATH)
                                     // FileName = "C:/path/to/ffmpeg/ffmpeg.exe", // Đường dẫn đến tệp thực thi FFmpeg (nếu chỉ định đường dẫn trực tiếp)
                UseShellExecute = false,
                RedirectStandardError = true
            };

            using (Process ffmpegProcess = new Process())
            {
                // Thời điểm bắt đầu ban đầu
                double currentTime = 0.0;

                // Xây dựng đoạn lệnh FFmpeg để chèn âm thanh vào video
                string ffmpegCommand = $"-i \"{inputVideoPath}\" ";

                foreach (string audioFile in audioFiles)
                {
                    // Kiểm tra xem tệp âm thanh tồn tại hay không
                    if (File.Exists(audioFile))
                    {
                        // Xây dựng đoạn lệnh chèn âm thanh
                        ffmpegCommand += $"-i \"{audioFile}\" -filter_complex \"[0:a][1:a]amerge[aout];[0:a]atrim=start={currentTime},asetpts=PTS-STARTPTS[av];[aout][av]amix=inputs=2:duration=shortest\" ";

                        // Tăng thời điểm bắt đầu cho tệp tiếp theo
                        currentTime += interval; // Thời gian giữa mỗi lần chèn âm thanh vào video
                    }
                    else
                    {
                        Console.WriteLine($"Không tìm thấy tệp âm thanh: {audioFile}");
                    }
                }

                // Xây dựng đoạn lệnh kết xuất video đầu ra
                ffmpegCommand += $"-map 0:v -map \"[aout]\" -c:v copy \"{outputVideoPath}\"";

                // Gán đoạn lệnh FFmpeg cho ProcessStartInfo
                ffmpegStartInfo.Arguments = ffmpegCommand;

                // Khởi chạy FFmpeg process
                ffmpegProcess.StartInfo = ffmpegStartInfo;
                ffmpegProcess.Start();

                // Chờ cho đến khi FFmpeg kết thúc
                ffmpegProcess.WaitForExit();
            }

        }
        public void InsertImagesIntoVideo(string videoPath, string[] imagePaths, string outputVideoPath)
        {
            // Khởi tạo VideoCapture để đọc video gốc
            using var videoCapture = new VideoCapture(videoPath);

            // Khởi tạo VideoWriter để ghi video mới
            var fourCC = FourCC.H264;
            var fps = videoCapture.Fps;
            var totalFrames = videoCapture.FrameCount;
            var frameSize = new OpenCvSharp.Size(videoCapture.FrameWidth, videoCapture.FrameHeight);
            using var writer = new VideoWriter(outputVideoPath, fourCC, fps, frameSize);
            Array.Sort(imagePaths);
            // Đọc hình ảnh và đảm bảo chúng có cùng kích thước với video
            var images = new List<Mat>();
            foreach (var imagePath in imagePaths)
            {
                var image = new Mat(imagePath);
                if (image.Width != frameSize.Width || image.Height != frameSize.Height)
                {
                    Cv2.Resize(image, image, new OpenCvSharp.Size(200, 200), 0, 0, InterpolationFlags.Linear);
                }
                images.Add(image);
            }
            int count = 0;
            // Đọc từng frame từ video gốc, chèn hình ảnh vào vị trí mong muốn, và ghi nó vào video mới
            int imageIndex = 0;
            for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {          
                using (var frame = new Mat())
                {
                    videoCapture.Read(frame); // đọc frame tiếp theo từ video gốc
                    count++;
                    if (frame.Empty()) // nếu không còn frame nào để đọc
                        break;
                    if (imageIndex >= images.Count)
                        imageIndex = 0;

                    // Nếu đây là frame mà bạn muốn chèn hình ảnh, thay thế frame bằng hình ảnh
                    if (0 <= count && count<= fps*0.5 && frameIndex > fps*2)
                    {
                        var roi = new Rect(250, 213, 200, 200);
                        images[imageIndex].CopyTo(frame[roi]);
                       
                    }
                    else if(frameIndex%(fps*2) == 0)
                    {
                        imageIndex++;
                        count = 0;
                    }
                    // Ghi frame vào video mới
                    writer.Write(frame);
                }
                
            }

            // Giải phóng tài nguyên
            foreach (var image in images)
            {
                image.Dispose();
            }
        }
    }

}
