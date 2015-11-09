using CodeClimate.Deb;
using CodeClimate.Wrapper;
using Rc.Framework;
using Rc.Framework.Yaml.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CodeClimate.CodeClimate.Runtime;

namespace CodeClimate
{
    public static class CodeClimate
    {
        public static string VersionModule = "2.6pr135";
        static CodeClimate ()
        {
            Terminal.ConfigTerminal conf = new Terminal.ConfigTerminal();
            conf.Header = "§4CodeClimate§f";
            conf.isUseColor = true;
            conf.isUseHeader = true;
            conf.isUseRCL = true;
            conf.VersionAPI = VTerminalAPI.v8_1;
            Terminal.SetConfig(conf);
            Terminal.Windows.Title = "CodeClimate - " + VersionModule;
        }
        private static Project ProjectConfig;
        private static Gradle GradleConfig;
        private static IDEA ideaConfig;
        private static Java javaConfig;
        public async static Task<bool> newProject(ConfigNewProject config)
        {
            Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f..");
            try
            {
                config.JavaHome = config.JavaHome.Replace("%JAVA_HOME%", Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine));
                config.PathHomeGradle = config.PathHomeGradle.Replace("%USER_PROFILE%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                if (!Directory.Exists(".cc"))
                {
                    DirectoryInfo inf = Directory.CreateDirectory(".cc");
                    inf.Attributes = FileAttributes.Hidden | FileAttributes.NotContentIndexed;
                }
                Project pr = new Project();
                Guid gu = Guid.NewGuid();
                pr.GUIDProject = gu.ToString();
                pr.arraySniffer = gu.ToByteArray();
                pr.ProjectID = config.ProjectID;
                pr.NameProject = config.NameProject;
                pr.Domen = config.Domen;
                pr.build = config.startBuild;
                pr.versionMajor = config.startVersionMajor;
                pr.versionMinor = config.startVersionMinor;
                pr.versionRef = config.startVersionRef;
                pr.versionState = config.startVersionState;
                pr.versionMine = config.versionMine;
                pr.versionForge = config.verionForge;

                Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f 14%..");

                new YamlSerializer().SerializeToFile($"{config.ProjectID.Replace(' ', '_')}.ccproj", pr);

                if(!File.Exists(".cc\\gradle.ccsln"))
                {
                    Gradle gradlConf = new Gradle();
                    gradlConf.HomePath = config.GetPathGradle();
                    gradlConf.Target = config.GetTargetGradle();
                    if (!File.Exists(config.GetTargetGradle()))
                    {
                        using (StreamWriter writer = File.CreateText(config.GetTargetGradle()))
                        {
                            writer.WriteLine($"project_domen={config.Domen}");
                            writer.WriteLine($"project_id={config.ProjectID}");
                            writer.WriteLine($"project_version={config.AssemblyVersion()}");
                            writer.WriteLine($"project_build={config.startBuild}");
                            writer.WriteLine($"project_mine_version={config.versionMine}");
                            writer.WriteLine($"project_forge_version={config.verionForge}");
                            writer.Flush();
                        }
                        Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f 31%..");
                    }
                    gradlConf.isOffline = config.isOfflineGradle;

                    new YamlSerializer().SerializeToFile($".cc\\gradle.ccsln", gradlConf);
                    File.SetAttributes($".cc\\gradle.ccsln", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                    Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f 56%..");
                }
                if (!File.Exists(".cc\\idea.ccsln"))
                {
                    IDEA idea = new IDEA();
                    idea.ArtifacteTarget = config.GetArtifacteTarget().Replace("%ProjectID%", config.ProjectID).Replace("%PROJECT_ROOT%", Environment.CurrentDirectory);

                    new YamlSerializer().SerializeToFile($".cc\\idea.ccsln", idea);
                    File.SetAttributes($".cc\\idea.ccsln", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                    Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f 65%..");
                }
                if(!File.Exists(".cc\\java.ccsln"))
                {
                    Java j = new Java();
                    j.PathHome = config.GetJavaHome();
                    j.JVMOpt = config.GetJVMOpt();
                    new YamlSerializer().SerializeToFile($".cc\\java.ccsln", j);
                    File.SetAttributes($".cc\\java.ccsln", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                    Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f 76%..");
                }
                if(!Directory.Exists(".cc\\wrapper"))
                {
                    DirectoryInfo inf = Directory.CreateDirectory(".cc\\wrapper");
                    inf.Attributes = FileAttributes.Hidden | FileAttributes.NotContentIndexed;
                    using (FileStream writer = File.Create(".cc\\wrapper\\gradle-wrapper.jar"))
                    {
                        await writer.WriteAsync(GradleWrapper.gradle_wrapper, 0, GradleWrapper.gradle_wrapper.Length);
                        Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f 95%..");
                    }
                    using (FileStream writer = File.Create(".cc\\wrapper\\gradle-wrapper.properties"))
                    {
                        await writer.WriteAsync(Prop.gradle_wrapper, 0, Prop.gradle_wrapper.Length);
                        Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstruct§f 97%..");
                    }
                    File.SetAttributes(".cc\\wrapper\\gradle-wrapper.jar", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                    File.SetAttributes(".cc\\wrapper\\gradle-wrapper.properties", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                }

                GradleConfig = YamlSerializer.Deserialize<Gradle>(".cc\\gradle.ccsln");
                ideaConfig = YamlSerializer.Deserialize<IDEA>(".cc\\idea.ccsln");
                javaConfig = YamlSerializer.Deserialize<Java>(".cc\\java.ccsln");

                RunGradle(GradlewRunType.setupWorkSpace);
                RunGradle(GradlewRunType.makeIdea);
                Terminal.WriteLine($"Project §8{config.NameProject}§f §aConstructed§f");
                return true;
            }
            catch (Exception ex)
            {
                Terminal.WriteLine(ex);
                return false;
            }
        }
        public static bool loadProject(string path)
        {
            try
            {
                ProjectConfig = YamlSerializer.Deserialize<Project>(path);
                GradleConfig = YamlSerializer.Deserialize<Gradle>(".cc\\gradle.ccsln");
                ideaConfig = YamlSerializer.Deserialize<IDEA>(".cc\\idea.ccsln");
                javaConfig = YamlSerializer.Deserialize<Java>(".cc\\java.ccsln");
                Terminal.Windows.Title = $"CodeClimate - {VersionModule} <{ProjectConfig.NameProject}({ProjectConfig.ProjectID}) - ";
                Terminal.Windows.Title += $"{ProjectConfig.AssemblyVersion()}-{ProjectConfig.versionMine}{ProjectConfig.versionForge}> Domen: {ProjectConfig.Domen}";
                Runtime.ShowGradleCop();
                Terminal.WriteLine($"Project §8{Path.GetFileName(path).Replace("_", " ")}§f §aloaded§f.");
                return true;
            }
            catch (Exception ex)
            {
                Terminal.WriteLine(ex);
                return false;
            }
        }
        public static bool saveProject()
        {
            try
            {
                if (File.Exists($".cc\\gradle.ccsln")) File.Delete($".cc\\gradle.ccsln");
                if (File.Exists($".cc\\idea.ccsln")) File.Delete($".cc\\idea.ccsln");
                if (File.Exists($".cc\\java.ccsln")) File.Delete($".cc\\java.ccsln");
                new YamlSerializer().SerializeToFile($"{ProjectConfig.ProjectID.Replace(' ', '_')}.ccproj", ProjectConfig);
                new YamlSerializer().SerializeToFile($".cc\\gradle.ccsln", GradleConfig);
                new YamlSerializer().SerializeToFile($".cc\\idea.ccsln", ideaConfig);
                new YamlSerializer().SerializeToFile($".cc\\java.ccsln", javaConfig);
                File.SetAttributes($".cc\\gradle.ccsln", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                File.SetAttributes($".cc\\idea.ccsln", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                File.SetAttributes($".cc\\java.ccsln", FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                Terminal.Windows.Title = $"CodeClimate - {VersionModule} <{ProjectConfig.NameProject}({ProjectConfig.ProjectID}) - ";
                Terminal.Windows.Title += $"{ProjectConfig.AssemblyVersion()}-{ProjectConfig.versionMine}{ProjectConfig.versionForge}> Domen: {ProjectConfig.Domen}";
                return true;
            }
            catch (Exception ex)
            {
                Terminal.WriteLine(ex);
                return false;
            }
        }
        public static void UpDateTarget()
        {
            if (File.Exists(GradleConfig.Target))
            {
                File.Delete(GradleConfig.Target);
                using (StreamWriter writer = File.CreateText(GradleConfig.Target))
                {
                    writer.WriteLine($"project_domen={ProjectConfig.Domen}");
                    writer.WriteLine($"project_id={ProjectConfig.ProjectID}");
                    writer.WriteLine($"project_version={ProjectConfig.AssemblyVersion()}");
                    writer.WriteLine($"project_build={ProjectConfig.build}");
                    writer.WriteLine($"project_mine_version={ProjectConfig.versionMine}");
                    writer.WriteLine($"project_forge_version={ProjectConfig.versionForge}");
                    writer.Flush();
                }
                Terminal.WriteLine($"Project §8{ProjectConfig.NameProject}§f: update §atarget§f gradlew.");
            }
        }
        public static class Runtime
        {
            public enum GradlewRunType
            {
                build,
                setupWorkSpace,
                makeIdea
            }
            public static Process ProcGradlew;
            public static Thread threadScan;
            private static object LockerBuild = new object();
            public static void ShowGradleCop()
            {
                Terminal.WriteLine("************* Gradle Tool ***************");
                Terminal.WriteLine("Powered: By MCP");
                Terminal.WriteLine("Url: http://modcoderpack.com/");
                Terminal.WriteLine("Author:");
                Terminal.WriteLine("       Searge, ProfMobius, Fesh0r,");
                Terminal.WriteLine("       R4wk, ZeuX, IngisKahn, bspkrs");
                Terminal.WriteLine("*****************************************");
            }
            public static void ShowVersion()
            {
                StringBuilder gradleArgs = new StringBuilder();

                foreach (string jopt in javaConfig.JVMOpt)
                {
                    gradleArgs.Append($"{jopt}" + " ");
                }
                gradleArgs.Append($"\"-Dorg.gradle.appname={Environment.CurrentDirectory}\"" + " ");
                gradleArgs.Append($"-classpath \"{Environment.CurrentDirectory}\\.cc\\wrapper\\gradle-wrapper.jar\"" + " ");
                gradleArgs.Append($"org.gradle.wrapper.GradleWrapperMain --version" + " ");

                if (GradleConfig.isOffline)
                    gradleArgs.Append($"--offline" + " ");

                ProcGradlew = new Process();
                Thread tProc = new Thread(() =>
                {
                    string strStar = "****************************";
                    string strTire = "------------------------------------------------------------";
                    string[] reader = new string[0];
                    int _error = default(int);
                    IL_24:
                    try
                    {
                        reader = ProcGradlew.StandardOutput.ReadToEnd().Replace(Environment.NewLine, "•").Split('•');
                    }
                    catch
                    {
                        if(_error >= 15)
                        {
                            var title = Terminal.GetConfig();
                            var titleDef = Terminal.GetConfig();
                            title.Header = "§4CLR§f";
                            Terminal.SetConfig(title);
                            Terminal.WriteLine("StandardOutput read error.");
                            Terminal.SetConfig(titleDef);
                            return;
                        }
                        _error++;
                        goto IL_24;
                    }
                    foreach(string s in reader)
                    {
                        string str = s;
                        if (str != null && str.Contains("Powered By MCP:")) str = null;
                        if (str != null && str.Contains("http://modcoderpack.com/")) str = null;
                        if (str != null && str.Contains("Searge, ProfMobius, Fesh0r,")) str = null;
                        if (str != null && str.Contains("R4wk, ZeuX, IngisKahn, bspkrs")) str = null;
                        if (str != null && str.Contains("MCP Data version : unknown")) str = null;
                        if (str != null && str.Contains("The assetDir is deprecated!")) str = null;
                        if (str != null && str.Contains("The runDir")) str = null;
                        if (str != null && str == strStar) str = null;
                        if (str != null && str == strTire) str = null;
                        if (str != null && str != "")
                        {
                            // Word
                            str = str.Replace("BUILD FAILED", "§cBUILD FAILED§f");
                            str = str.Replace("FAILURE", "§cFAILURE§f");
                            str = str.Replace("UP-TO-DATE", "§6UP-TO-DATE§f");
                            str = str.Replace("SKIPPED", "§3SKIPPED§f");
                            str = str.Replace("SUCCESSFUL", "§aSUCCESSFUL§f");
                            str = str.Replace("OS", "§aOS§f");
                            str = str.Replace("JVM", "§dJVM§f");
                            str = str.Replace("Groovy", "§bGroovy§f");
                            str = str.Replace("Ant", "§9Ant§f");
                            // Except text
                            str = str.Replace("Build failed with an exception", "§6Build failed with an exception§f");
                            // Symbol
                            str = str.Replace(">", "§6>§f");

                            Terminal.WriteLine($"{str}");
                            str = null;
                        }
                    }
                });
                ProcGradlew.StartInfo = new ProcessStartInfo($"{javaConfig.PathHome}\\bin\\java.exe")
                {
                    Arguments = gradleArgs.ToString(),
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                tProc.Start();
                ProcGradlew.Start();
                //x ProcGradlew.WaitForExit();
                tProc.Join();
            }
            public static bool RunGradle(GradlewRunType t)
            {
                lock(LockerBuild)
                {
                    try
                    {
                        StringBuilder gradleArgs = new StringBuilder();
                        DateTime dtStartBuild = DateTime.Now;
                        if (t == GradlewRunType.build)
                        {
                            Terminal.WriteLine($"Start §ebuild§f: {dtStartBuild}.");
                            foreach (string jopt in javaConfig.JVMOpt)
                            {
                                gradleArgs.Append($"{jopt}" + " ");
                            }
                            gradleArgs.Append($"\"-Dorg.gradle.appname={Environment.CurrentDirectory}\"" + " ");
                            gradleArgs.Append($"-classpath \"{Environment.CurrentDirectory}\\.cc\\wrapper\\gradle-wrapper.jar\"" + " ");
                            gradleArgs.Append($"org.gradle.wrapper.GradleWrapperMain --gradle-user-home" + " ");
                            gradleArgs.Append($"\"{GradleConfig.HomePath}\"" + " ");

                            gradleArgs.Append($"build jar" + " ");

                            if (GradleConfig.isOffline)
                                gradleArgs.Append($"--offline" + " ");
                            if (GradleConfig.isInfo)
                                gradleArgs.Append($"--info" + " ");
                            if (GradleConfig.isDebug)
                                gradleArgs.Append($"--debug" + " ");
                        }
                        else if (t == GradlewRunType.makeIdea)
                        {
                            Terminal.WriteLine($"Start §emake§f idea: {dtStartBuild}.");
                            foreach (string jopt in javaConfig.JVMOpt)
                            {
                                gradleArgs.Append($"{jopt}" + " ");
                            }
                            gradleArgs.Append($"\"-Dorg.gradle.appname={Environment.CurrentDirectory}\"" + " ");
                            gradleArgs.Append($"-classpath \"{Environment.CurrentDirectory}\\.cc\\wrapper\\gradle-wrapper.jar\"" + " ");
                            gradleArgs.Append($"org.gradle.wrapper.GradleWrapperMain --gradle-user-home" + " ");
                            gradleArgs.Append($"'{GradleConfig.HomePath}'" + " ");

                            gradleArgs.Append($"idea" + " ");

                            if (GradleConfig.isOffline)
                                gradleArgs.Append($"--offline" + " ");
                            if (GradleConfig.isInfo)
                                gradleArgs.Append($"--info" + " ");
                            if (GradleConfig.isDebug)
                                gradleArgs.Append($"--debug" + " ");
                        }
                        else if (t == GradlewRunType.setupWorkSpace)
                        {
                            Terminal.WriteLine($"Start §esetup§f Work Space: {dtStartBuild}.");
                            foreach (string jopt in javaConfig.JVMOpt)
                            {
                                gradleArgs.Append($"{jopt}" + " ");
                            }
                            gradleArgs.Append($"\"-Dorg.gradle.appname={Environment.CurrentDirectory}\"" + " ");
                            gradleArgs.Append($"-classpath \"{Environment.CurrentDirectory}\\.cc\\wrapper\\gradle-wrapper.jar\"" + " ");
                            gradleArgs.Append($"org.gradle.wrapper.GradleWrapperMain --gradle-user-home" + " ");
                            gradleArgs.Append($"'{GradleConfig.HomePath}'" + " ");

                            gradleArgs.Append($"setupDecompWorkspace" + " ");

                            if (GradleConfig.isOffline)
                                gradleArgs.Append($"--offline" + " ");
                            if (GradleConfig.isInfo)
                                gradleArgs.Append($"--info" + " ");
                            if (GradleConfig.isDebug)
                                gradleArgs.Append($"--debug" + " ");
                        }
                        ShowVersion();
                        ProcGradlew = new Process();
                        ProcGradlew.StartInfo = new ProcessStartInfo($"{javaConfig.PathHome}\\bin\\java.exe")
                        {
                            Arguments = gradleArgs.ToString(),
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        };
                        Thread tProc = new Thread(() =>
                        {
                            string strStar = "****************************";
                            string strTire = "------------------------------------------------------------";
                            string str = null;
                            while (true)
                            {
                                try
                                {
                                    int i = ProcGradlew.ExitCode;
                                    return;
                                }
                                catch { }
                                Thread.Sleep(5);
                                try
                                {
                                    if (ProcGradlew.StartTime == null)
                                        continue;
                                    str = ProcGradlew.StandardOutput.ReadLine();
                                }
                                catch (ThreadAbortException)
                                { return; }
                                try
                                {
                                    if (str != null && str.Contains("Powered By MCP:")) str = null;
                                    if (str != null && str.Contains("http://modcoderpack.com/")) str = null;
                                    if (str != null && str.Contains("Searge, ProfMobius, Fesh0r,")) str = null;
                                    if (str != null && str.Contains("R4wk, ZeuX, IngisKahn, bspkrs")) str = null;
                                    if (str != null && str.Contains("MCP Data version : unknown")) str = null;
                                    if (str != null && str.Contains("The assetDir is deprecated!")) str = null;
                                    if (str != null && str.Contains("The runDir")) str = null;
                                    if (str != null && str == strStar) str = null;
                                    if (str != null && str == strTire) str = null;
                                    if (str != null && str != "")
                                    {
                                        // Word
                                        str = str.Replace("BUILD FAILED", "§cBUILD FAILED§f");
                                        str = str.Replace("FAILED", "§cFAILED§f");
                                        str = str.Replace("FAILURE", "§cFAILURE§f");
                                        str = str.Replace("UP-TO-DATE", "§6UP-TO-DATE§f");
                                        str = str.Replace("SKIPPED", "§3SKIPPED§f");
                                        str = str.Replace("SUCCESSFUL", "§aSUCCESSFUL§f");
                                        str = str.Replace("OS", "§aOS§f");
                                        str = str.Replace("JVM", "§dJVM§f");
                                        str = str.Replace("Groovy", "§bGroovy§f");
                                        str = str.Replace("Ant", "§9Ant§f");
                                        // Except text
                                        str = str.Replace("Build failed with an exception", "§6Build failed with an exception§f");
                                        // Symbol
                                        str = str.Replace(">", "§6>§f");

                                        Terminal.WriteLine($"{str}");
                                        str = null;
                                    }
                                }
                                catch
                                { continue; }
                            }
                        });
                        tProc.Start();
                        ProcGradlew.Start();
                        Terminal.WriteLine($"Configure §agradle§f.");
                        ProcGradlew.WaitForExit();
                        if(ProcGradlew.ExitCode == 0)
                        {
                            ProjectConfig.build++;
                            UpDateTarget();
                            saveProject();
                        }
                        tProc.Abort();
                        tProc = null;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Terminal.WriteLine(ex);
                        return false;
                    }
                }
            }

            public static void StartScan()
            {
                threadScan = new Thread(scan_artifacte);
                threadScan.Name = "[CodeClimate] Scanner artifacte.";
                threadScan.Start();
            }
            private static void scan_artifacte()
            {
                DateTime TimeOld;
                if (!File.Exists(ideaConfig.ArtifacteTarget))
                    TimeOld = default(DateTime);
                else
                    TimeOld = File.GetLastWriteTime(ideaConfig.ArtifacteTarget);
                while (true) 
                {
                    Thread.Sleep(15);
                    if (!File.Exists(ideaConfig.ArtifacteTarget))
                        continue;
                    DateTime time = File.GetLastWriteTime(ideaConfig.ArtifacteTarget);

                    if(TimeOld < time)
                    {
                        Terminal.WriteLine("Detected §9new§f artifact.");
                        Terminal.WriteLine("Sleep 4 sec...");
                        Thread.Sleep(4000);
                        bool succees = RunGradle(GradlewRunType.build);
                        TimeOld = time;
                    }
                }
            }
        }
    }
}
