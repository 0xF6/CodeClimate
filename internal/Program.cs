
using CodeClimate.Deb;
using Rc.Framework;
using Rc.Framework.Extension;
using Rc.Framework.Yaml.Serialization;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static CodeClimate.CodeClimate;

namespace CodeClimate
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            //new YamlSerializer().SerializeToFile("c.c", new ConfigNewProject());
            Terminal.WriteLine($"AGTSoul [CodeClimate {CodeClimate.VersionModule}] [RC.Core {Framework.Version}]");
            Terminal.WriteLine($"(C) Продвинутые игровые технологии (AGTSoul), 2015 г. Все права не защищены.\n");
            for (int i = 0; i != args.Length; i++)
            {
                if (new Regex("(\\.ccproj)").IsMatch(args[i]))
                {
                    Terminal.WriteLine("case §accproj§f: load project, start scan..");
                    loadProject(args[i]);
                    Runtime.StartScan();
                    break;
                }
                else if (new Regex("(\\.ccpconf)").IsMatch(args[i]))
                {
                    Terminal.WriteLine("case §accpconf§f: load config, create project..");
                    var p = YamlSerializer.Deserialize<ConfigNewProject>(args[i]);
                    Thread t = new Thread(async () => { await newProject(p); });
                    t.Start();
                    t.Join();
                    break;
                }
            }
            if (args.Length == 0) 
            {
                bool isNewProject = false;
                bool isExistProject = false;
                string pathToConfigNewProject = "";
                string pathToExistProject = "";
                Terminal.WriteLine("Line Args Empty!");
                Terminal.WriteLine("Search is this directory...");
                string[] sts = Directory.GetFiles(Environment.CurrentDirectory);
                foreach (string s in sts)
                {
                    if (new Regex("(\\.ccpconf)").IsMatch(s))
                    {
                        Terminal.WriteLine("case §accpconf§f! this§eOperation§f: new project!");
                        isNewProject = true;
                        pathToConfigNewProject = s;
                    }
                    if (new Regex("(\\.ccproj)").IsMatch(s))
                    {
                        if(isNewProject)
                            Terminal.WriteLine("case §accproj§f! Exist Project.. amm.. and case §accpconf§f, ohh.. this§eOperation§f: load project.");
                        else
                            Terminal.WriteLine("case §accproj§f! Exist Project! this§eOperation§f: load project.");
                        isExistProject = true;
                        pathToExistProject = s;
                    }
                }
                if(isNewProject && !isExistProject)
                {
                    var p = YamlSerializer.Deserialize<ConfigNewProject>(pathToConfigNewProject);
                    Thread t = new Thread(async () => 
                    {
                        await newProject(p);
                    });
                    t.Start();
                    t.Join();
                }
                if (isNewProject && isExistProject || !isNewProject && isExistProject)
                {
                    loadProject(pathToExistProject);
                    Runtime.StartScan();
                }
            }
            Terminal.Pause();
        }
    }
}
