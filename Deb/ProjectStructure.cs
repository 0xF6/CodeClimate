using Rc.Framework;
using Rc.Framework.Extension;
using Rc.Framework.Yaml.Serialization;
using System;
using System.Collections.Generic;
namespace CodeClimate.Deb
{
    public static class VersionMine
    {
        public const string v1_7_10 = "1.7.10";
    }
    public static class VersionForge
    {
        public const string v10_13_0_1180 = "10.13.0.1180";
    }
    public enum VersionGradle
    {
        v2_0,
        v2_7
    }
    public enum VersionState
    {
        alpha,
        beta,
        gamma,
        prerelease,
        release
    }

    [Yaml(CompactMethod.Assign)]
    public class Project
    {
        public string NameProject;
        public string ProjectID;
        public string GUIDProject;
        public byte[] arraySniffer;
        public string Domen;
        public int build;
        public int versionMajor;
        public int versionMinor;
        public int versionRef;
        public string versionState;
        public string versionForge;
        public string versionMine;
    }
    [Yaml(CompactMethod.Assign)]
    public class Gradle
    {
        public string Opt;
        public string Target;
        public string HomePath;
        public bool isOffline;
        public bool isInfo;
        public bool isDebug;
    }
    //# i'am hate Eclipse
    [Yaml(CompactMethod.Assign)] public class Eclipse 
    {
        public string ArtifacteTarget;
    }
    [Yaml(CompactMethod.Assign)] public class IDEA
    {
        public string ArtifacteTarget;
    }
    [Yaml(CompactMethod.Assign)] public class Java
    {
        public string PathHome;
        public string[] JVMOpt;
    }
    [Yaml(CompactMethod.Assign)] public class ConfigNewProject
    {
        public string NameProject;
        public string ProjectID;
        public int startBuild = 0;
        public int startVersionMajor = 1;
        public int startVersionMinor = 0;
        public int startVersionRef = 0;
        public string startVersionState;
        public string Domen;
        public bool isOfflineGradle;
        public string verionForge = VersionForge.v10_13_0_1180;
        public string versionMine = VersionMine.v1_7_10;

        public string[] JVMOpt = new string[0];
        public string[] GradleOpt = new string[0];
        public string JavaHome = "%JAVA_HOME%";
        public string TargetGradle = "build.gradlew.target";
        public string ArtifacteTarget = $"%PROJECT_ROOT%\\build\\artifacte\\%ProjectID%.jar";
        public string PathHomeGradle = "%USER_PROFILE%\\.gradle";

        public ConfigNewProject()
        {
            List<string> ListJVMOpt = new List<string>(JVMOpt);
            ListJVMOpt.Add("");
            JVMOpt = ListJVMOpt.ToArray();

            List<string> ListGradleOpt = new List<string>(GradleOpt);
            ListGradleOpt.Add("");
            GradleOpt = ListGradleOpt.ToArray();
        }
        public void AddJVMOpt(string opt)
        {
            List<string> ListJVMOpt = new List<string>(JVMOpt);
            if (!ListJVMOpt.Contains(opt))
            {
                ListJVMOpt.Add(opt);
                JVMOpt = ListJVMOpt.ToArray();
            }
        }
        public void AddGradleOpt(string opt)
        {
            List<string> ListGradleOpt = new List<string>(GradleOpt);
            if (!ListGradleOpt.Contains(opt))
            {
                ListGradleOpt.Add(opt);
                GradleOpt = ListGradleOpt.ToArray();
            }
        }
        public string[] GetJVMOpt()
        {
            return JVMOpt;
        }
        public string[] GetGradleOpt()
        {
            return GradleOpt;
        }
        public string GetJavaHome()
        {
            return JavaHome;
        }
        public string GetTargetGradle()
        {
            return TargetGradle;
        }
        public string GetArtifacteTarget()
        {
            return ArtifacteTarget;
        }
        public string GetPathGradle()
        {
            return PathHomeGradle;
        }
        //
        public void SetTargetGradle(string value)
        {
            TargetGradle = value;
        }
        public void SetArtifacteTarget(string value)
        {
            ArtifacteTarget = value;
        }
        public void SetPathGradle(string value)
        {
            PathHomeGradle = value;
        }
    }
    public static class Ex
    {
        public static string AssemblyVersion(this ConfigNewProject conf)
        {
            string s = $"{conf.startVersionMajor}.{conf.startVersionMinor}";
            try
            {
                if (conf.startVersionState.ToEnum<VersionState>() == VersionState.alpha)
                    s = $"{s}a";
                if (conf.startVersionState.ToEnum<VersionState>() == VersionState.beta)
                    s = $"{s}b";
                if (conf.startVersionState.ToEnum<VersionState>() == VersionState.gamma)
                    s = $"{s}g";
                if (conf.startVersionState.ToEnum<VersionState>() == VersionState.prerelease)
                    s = $"{s}pre";
                if (conf.startVersionState.ToEnum<VersionState>() == VersionState.release)
                    s = $"{s}r";
                s = $"{s}{conf.startVersionRef}";
            }
            catch
            {
                var d = (IDictionary<string, string>)new Dictionary<string, string>();
                d.Add("Project State", "§cINIT§f");
                throw new RException<String, String>("Prop 'startVersionState' is not valid, check list 'VersionState.yaml'", d);
            }
            return s;
        }
        public static string AssemblyVersion(this Project conf)
        {
            string s = $"{conf.versionMajor}.{conf.versionMinor}";
            try
            {
                if (conf.versionState.ToEnum<VersionState>() == VersionState.alpha)
                    s = $"{s}a";
                if (conf.versionState.ToEnum<VersionState>() == VersionState.beta)
                    s = $"{s}b";
                if (conf.versionState.ToEnum<VersionState>() == VersionState.gamma)
                    s = $"{s}g";
                if (conf.versionState.ToEnum<VersionState>() == VersionState.prerelease)
                    s = $"{s}pre";
                if (conf.versionState.ToEnum<VersionState>() == VersionState.release)
                    s = $"{s}r";
                s = $"{s}{conf.versionRef}";
            }
            catch
            {
                var d = (IDictionary<string, string>)new Dictionary<string, string>();
                d.Add("Project State", $"§9ACTIVE§f");
                var e = new RException<String, String>("Prop 'versionState' is not valid, check list 'VersionState.yaml'", d);
                Terminal.WriteLine(e);
            }
            return s;
        }
    }
}
