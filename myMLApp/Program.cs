using Microsoft.ML;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace myMLApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var runProg = true;
            var d = DateTime.Now.Year;
            Console.WriteLine($"Image Classification Sorter\n©{d}™T©ReMeTaL\nInitializing please wait");
            if (!FileSystem.FileExists("firstrun.tmp"))
            {
                if (Environment.CurrentDirectory != AppDomain.CurrentDomain.BaseDirectory)
                {
                    try
                    {
                        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Smart-Sort", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (!FileSystem.DirectoryExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures"))
                {
                    FileSystem.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures");
                }
                if (!FileSystem.DirectoryExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\PictureSort"))
                {
                    FileSystem.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\PictureSort");
                }
                if (MessageBox.Show($"Add Folders containing images to [{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures] to use for sort training\nExamples:\n{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\cats \n{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\dogs\nThe {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures folder can not contain any files only folders during training\nThe folder names will be used as the sort labels for sorting pictures\n place images to be sorted in the [{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\PictureSort] folder\n Press Ok when finished to begin AI Training or cancel to exit and train later.", "Smart-Sort", System.Windows.Forms.MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    if (FileSystem.DirectoryExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures"))
                    {
                        if (FileSystem.GetDirectories($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures").Count > 0)
                        {
                            Console.WriteLine("Training Sort AI");
                            var mlContext = new MLContext();
                            // Define DataViewSchema of data prep pipeline and trained model
                            _ = mlContext.Model.Load("SentimentModel.zip", out DataViewSchema modelSchema);
                            //Load New Data
                            IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures", useFolderNameAsLabel: true);
                            IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
                            ITransformer retrainedModel = MyMLApp.SentimentModel.RetrainPipeline(mlContext, imageData);
                            mlContext.Model.Save(retrainedModel, modelSchema, Path.GetFullPath("SentimentModel.zip"));
                            Console.WriteLine("Training Complete");
                            var fwtr = FileSystem.OpenTextFileWriter("firstrun.tmp", false);
                            fwtr.WriteLine("true");
                            fwtr.Close();
                            fwtr.Dispose();
                        }
                        else
                        {
                            runProg = false;
                            Console.WriteLine($"The {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures folder has no sub-folders");
                        }
                    }
                    else { runProg = false; }
                }
                else { runProg = false; }
            }
            if (runProg)
            {
                int gscr = 55;
                if (args.GetLength(0) > 0)
                {
                    if (FileSystem.DirectoryExists($"{args[0]}"))
                    {
                        if (MessageBox.Show($"Sort Directory? {args[0]}", "Smart-Sort", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                        {
                            Form1 myform = new();
                            myform.Show();
                            Console.WriteLine("Sorting Directory: " + args[0].ToString() + "\n");
                            //Environment.SetEnvironmentVariable("chdir", " /d %~dp0");
                            //Environment.ExpandEnvironmentVariables("%~dp0");
                            try
                            {
                                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message, "Smart-Sort", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            try
                            {
                                gscr = int.Parse(args[1]);
                            }
                            catch (Exception)
                            {
                                gscr = 55;
                            }
                            Console.WriteLine($"\nMinimum Match Accuracy:{gscr}");
                            try
                            {
                                foreach (var item in FileSystem.GetFiles($"{args[0]}"))
                                {

                                    //Console.WriteLine(item.Replace(FileSystem.GetParentPath(item) + "\\", "") + "\n");
                                    MyMLApp.SentimentModel.ModelInput sampleData = new()
                                    {
                                        ImageSource = @item.ToString(),
                                    };
                                    var predictionResult = MyMLApp.SentimentModel.Predict(sampleData);
                                    float scoreStr = predictionResult.Score[0];
                                    foreach (var labs in predictionResult.Score)
                                    {
                                        if (labs > scoreStr)
                                        {
                                            scoreStr = labs;
                                        }
                                    }
                                    int scr = (int)(scoreStr * 100);
                                    Console.WriteLine("\nImage: " + item.Replace(FileSystem.GetParentPath(item) + "\\", "") + "\n");
                                    Console.WriteLine($"Predicted Label value: {predictionResult.Prediction} Match Rate: {scr}%\nPredicted Label scores: [{String.Join(",", predictionResult.Score)}]\n\n");
                                    if (scr > gscr)
                                    {
                                        /*if (!FileSystem.DirectoryExists($"{args[0]}\\PictureSort"))
                                        {
                                            Console.WriteLine($"Creating Directory: {args[0]}\\PictureSort\n");
                                            FileSystem.CreateDirectory($"{args[0]}\\PictureSort");
                                        }*/
                                        if (!FileSystem.DirectoryExists($"{args[0]}\\{predictionResult.Prediction}"))
                                        {
                                            Console.WriteLine($"Creating Directory: {args[0]}\\{predictionResult.Prediction}\n");
                                            FileSystem.CreateDirectory($"{args[0]}\\{predictionResult.Prediction}");
                                        }
                                        var theNewfile = $"{args[0]}\\{predictionResult.Prediction}\\{item.Replace(FileSystem.GetParentPath(item) + "\\", "")}";
                                        if (FileSystem.FileExists($"{args[0]}\\{predictionResult.Prediction}\\{item.Replace(FileSystem.GetParentPath(item) + "\\", "")}"))
                                        {
                                            int fcnt = 1;
                                            while (FileSystem.FileExists($"{args[0]}\\{predictionResult.Prediction}\\({fcnt}){item.Replace(FileSystem.GetParentPath(item) + "\\", "")}"))
                                            {
                                                fcnt++;
                                            }
                                            theNewfile = $"{args[0]}\\{predictionResult.Prediction}\\({fcnt}){item.Replace(FileSystem.GetParentPath(item) + "\\", "")}";
                                        }
                                        FileSystem.MoveFile(item.ToString(), $"{theNewfile}");
                                        if (FileSystem.FileExists(theNewfile))
                                        {
                                            Console.WriteLine($"Moved {item} to {theNewfile}\n");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Error Moving {item} to {theNewfile}\n");
                                        }
                                        //Console.WriteLine($"Moving {item} to {args[0]}\\{predictionResult.Prediction}\\{item.Replace(FileSystem.GetParentPath(item) + "\\", "")}\n");
                                        //FileSystem.MoveFile(item.ToString(), $"{args[0]}\\{predictionResult.Prediction}\\{item.Replace(FileSystem.GetParentPath(item) + "\\", "")}");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message, "Smart-Sort", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            if (myform != null)
                            {
                                myform.Close();
                                myform.Dispose();
                            }
                            MessageBox.Show("Sorting Completed!", "Smart-Sort", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show($"Have you created any new folders in the \n[{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures]\nfolder or added images to any of the existing folders in it and need to re-train the Sort-AI before Sorting?", "Smart-Sort", System.Windows.Forms.MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (FileSystem.DirectoryExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures"))
                        {
                            if (FileSystem.GetDirectories($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures").Count > 0)
                            {
                                var mlContext = new MLContext();
                                // Define DataViewSchema of data prep pipeline and trained model
                                _ = mlContext.Model.Load("SentimentModel.zip", out DataViewSchema modelSchema);
                                //Load New Data
                                IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures", useFolderNameAsLabel: true);
                                IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
                                ITransformer retrainedModel = MyMLApp.SentimentModel.RetrainPipeline(mlContext, imageData);
                                mlContext.Model.Save(retrainedModel, modelSchema, Path.GetFullPath("SentimentModel.zip"));
                                //Console.Write("Training Complete\nPress any key to close this window . . .");
                                //Console.ReadKey();
                                MessageBox.Show("Training Complete!", "Smart-Sort", MessageBoxButtons.OK);
                            }
                            else
                            {
                                MessageBox.Show($"No Image folders located in the {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures folder to use for training!\nExamples:\n{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\cats \n{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\dogs", "Smart-Sort", System.Windows.Forms.MessageBoxButtons.OK);
                            }
                        }
                    }

                    if (MessageBox.Show($"Sort Directory? {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\PictureSort", "Smart-Sort", System.Windows.Forms.MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                    {
                        Form1 myform = new();
                        myform.Show();
                        using (Prompt prompt = new($"Images with a match Accuracy less than (Current: {gscr}) do not get moved.\nSet Image Match Accuracy (0-100)", "Set Image Match Accuracy (0-100)"))
                        {
                            string result = prompt.Result;
                            if (result != "")
                            {
                                try
                                {
                                    //gscr = Int32.Parse(gscrs);
                                    gscr = int.Parse(result);
                                    if (gscr > 100) { gscr = 100; }
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("error setting match rate; not a number");
                                    gscr = 55;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Using Default");
                            }
                        }
                        Console.WriteLine($"\nMinimum Match Accuracy:{gscr}");
                        //Console.ReadKey();
                        Console.WriteLine($"Sorting Directory: {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\PictureSort\n");
                        foreach (var item in FileSystem.GetFiles($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\PictureSort"))
                        {
                            MyMLApp.SentimentModel.ModelInput sampleData = new()
                            {
                                ImageSource = @item.ToString(),
                            };
                            // Make a single prediction on the sample data and print results
                            var predictionResult = MyMLApp.SentimentModel.Predict(sampleData);
                            var scoreStr = predictionResult.Score[0];
                            foreach (var labs in predictionResult.Score)//Directory.EnumerateDirectories(Path.GetDirectoryName($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures"))
                            {
                                if (labs > scoreStr)
                                {
                                    scoreStr = labs;
                                }
                            }
                            int scr = (int)(scoreStr * 100);// \n{scr}%\n{predictionResult.Score[scoreStr]}
                            Console.WriteLine("\nImage: " + item.Replace(FileSystem.GetParentPath(item) + "\\", "") + "\n");
                            Console.WriteLine($"Predicted Label value: {predictionResult.Prediction} Match Rate: {scr}%\nPredicted Label scores: [{String.Join(",", predictionResult.Score)}]\n\n");
                            if (scr > gscr)
                            {
                                if (!FileSystem.DirectoryExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures"))
                                {
                                    Console.WriteLine($"Creating Directory: {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\n");
                                    FileSystem.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures");
                                }
                                if (!FileSystem.DirectoryExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\{predictionResult.Prediction}"))
                                {
                                    Console.WriteLine($"Creating Directory: {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\{predictionResult.Prediction}\n");
                                    FileSystem.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\{predictionResult.Prediction}");
                                }
                                var theNewfile = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\{predictionResult.Prediction}\\{item.Replace(FileSystem.GetParentPath(item) + "\\", "")}";
                                if (FileSystem.FileExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\{predictionResult.Prediction}\\{item.Replace(FileSystem.GetParentPath(item) + "\\", "")}"))
                                {
                                    int fcnt = 1;
                                    while (FileSystem.FileExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\{predictionResult.Prediction}\\({fcnt}){item.Replace(FileSystem.GetParentPath(item) + "\\", "")}"))
                                    {
                                        fcnt++;
                                    }
                                    theNewfile = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures\\{predictionResult.Prediction}\\({fcnt}){item.Replace(FileSystem.GetParentPath(item) + "\\", "")}";
                                }
                                FileSystem.MoveFile(item.ToString(), $"{theNewfile}");
                                if (FileSystem.FileExists(theNewfile))
                                {
                                    Console.WriteLine($"Moved {item} to {theNewfile}\n");
                                }
                                else
                                {
                                    Console.WriteLine($"Error Moving {item} to {theNewfile}\n");
                                }
                            }
                        }
                        if (myform != null)
                        {
                            myform.Close();
                            myform.Dispose();
                        }
                        MessageBox.Show("Sorting Complete!", "Smart-Sort", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Sort Cancelled!", "Smart-Sort", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                /*Console.Write("Sort Complete!\nRetrain? (y) or Press any key to close this window . . .");
                if (Console.ReadKey(true).KeyChar == 'y')
                {
                    Console.WriteLine("Retraining:\n");
                    if (FileSystem.DirectoryExists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures"))
                    {
                        var mlContext = new MLContext();
                        // Define DataViewSchema of data prep pipeline and trained model
                        ITransformer trainedModel = mlContext.Model.Load("SentimentModel.zip", out DataViewSchema modelSchema);
                        //Load New Data
                        IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Pictures", useFolderNameAsLabel: false);
                        IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
                        ITransformer retrainedModel = SentimentModel.RetrainPipeline(mlContext, imageData);
                        mlContext.Model.Save(retrainedModel, modelSchema, Path.GetFullPath("SentimentModel.zip"));
                        Console.Write("Retraining Complete!\nPress any key to close this window . . .");
                        Console.ReadKey();
                    }
                };*/

                //Console.WriteLine("\nSort Complete!\nPress any key to close this window . . .");
                //Console.ReadKey();

            }
            else
            {
                Console.WriteLine("Initial Training Needed before images can be sorted.");
            }
        }

        public static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
        {
            var files = Directory.GetFiles(folder, "*",
                searchOption: System.IO.SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
                    continue;

                var label = Directory.GetParent(file).Name;

                if (!useFolderNameAsLabel)
                {
                    label = Path.GetFileName(file).Replace(Path.GetExtension(file), "");
                }
                /*else
                {
                    for (int index = 0; index < label.Length; index++)
                    {
                        if (!char.IsLetter(label[index]))
                        {
                            label = label[..index];
                            break;
                        }
                    }
                }*/

                yield return new ImageData()
                {
                    ImageSource = file,
                    Label = label
                };
            }
        }
    }

    class ImageData
    {
        public string Label { get; set; }

        public string ImageSource { get; set; }

    }
    public class Prompt : IDisposable
    {
        private Form UserPrompt { get; set; }
        public string Result { get; }

        public Prompt(string text, string caption)
        {
            Result = ShowDialog(text, caption);
        }
        //use a using statement
        private string ShowDialog(string text, string caption)
        {
            UserPrompt = new Form()
            {
                Width = 500,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true,
                Icon = Form1.ActiveForm.Icon
            };
            Label textLabel = new() { Left = 50, Top = 35, Text = text, Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, Height = 35 };
            TextBox textBox = new() { Left = 200, Top = 50, Width = 100, Text = "55", TextAlign = HorizontalAlignment.Center };
            Button confirmation = new() { Text = "Set Rate", Left = 120, Width = 100, Top = 100, DialogResult = DialogResult.OK };
            Button cancel = new() { Text = $"Use Default {textBox.Text}", Left = 280, Width = 100, Top = 100, DialogResult = DialogResult.Cancel };
            confirmation.Click += (sender, e) => { UserPrompt.Close(); };
            cancel.Click += (sender, e) => { UserPrompt.Close(); };
            textBox.TextChanged += (sender, e) => { confirmation.Text = $"Set Rate {textBox.Text}"; };
            UserPrompt.Controls.Add(textBox);
            UserPrompt.Controls.Add(confirmation);
            UserPrompt.Controls.Add(cancel);
            UserPrompt.Controls.Add(textLabel);
            UserPrompt.AcceptButton = confirmation;

            return UserPrompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
        public void Dispose()
        {
            //See Marcus comment
            if (UserPrompt != null)
            {
                UserPrompt.Dispose();
            }
        }
    }
}