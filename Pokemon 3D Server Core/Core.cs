﻿using Amib.Threading;
using Modules.System;
using Pokemon_3D_Server_Core.Interface;
using Pokemon_3D_Server_Core.Server.Game.Server;
using Pokemon_3D_Server_Core.Server.Game.World;
using Pokemon_3D_Server_Core.SQLite;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pokemon_3D_Server_Core
{
    public class Core
    {
        /// <summary>
        /// Get all active modules.
        /// </summary>
        public static List<IModules> ActiveModules { get; private set; } = new List<IModules>();

        /// <summary>
        /// Get Application Settings.
        /// </summary>
        public static Settings.Settings Settings { get; set; }

        /// <summary>
        /// Get Application Logger.
        /// </summary>
        public static Logger.Logger Logger { get; private set; }

        /// <summary>
        /// Get Application Database.
        /// </summary>
        public static Database Database { get; private set; }

        #region Server
        #region Game
        #region Server
        /// <summary>
        /// Get Game Server Listener.
        /// </summary>
        public static Listener Listener { get; private set; }

        /// <summary>
        /// Get Game Server TcpClientCollection.
        /// </summary>
        public static TcpClientCollection TcpClientCollection { get; private set; }
        #endregion Server

        #region World
        /// <summary>
        /// Get Game World.
        /// </summary>
        public static World World { get; private set; }
        #endregion World
        #endregion Game
        #endregion Server

        /// <summary>
        /// Get Start Argument.
        /// </summary>
        public string[] StartArgument { get; private set; }

        private bool isActive = false;
        private IWorkItemsGroup ThreadPool = new SmartThreadPool().CreateWorkItemsGroup(1);

        public Core(string[] args, params IModules[] modules)
        {
            StartArgument = args;

            Settings = new Settings.Settings();
            Database = new Database();
            Logger = new Logger.Logger();

            Listener = new Listener();
            TcpClientCollection = new TcpClientCollection();
            World = new World();

            ActiveModules.Add(Settings);
            ActiveModules.Add(Database);
            ActiveModules.AddRange(modules);
            ActiveModules.Add(Logger);

            isActive = true;
        }

        /// <summary>
        /// Start Core Application.
        /// </summary>
        public void Start()
        {
            ThreadPool.QueueWorkItem(() =>
            {
                while (!isActive)
                    Thread.Sleep(1000);

                foreach (IModules item in ActiveModules)
                {
                    try
                    {
                        item.Start();
                    }
                    catch (Exception ex)
                    {
                        ex.CatchError();
                    }

                    Logger.Log($"Module: {item.Name} v{item.Version} is started.");
                }
            });
        }

        /// <summary>
        /// Stop Core Application.
        /// </summary>
        public void Stop()
        {
            ThreadPool.QueueWorkItem(() =>
            {
                while (!isActive)
                    Thread.Sleep(1000);

                foreach (IModules item in ActiveModules)
                {
                    try
                    {
                        item.Stop();
                    }
                    catch (Exception ex)
                    {
                        ex.CatchError();
                    }

                    Logger.Log($"Module: {item.Name} v{item.Version} is stopped.");
                }
            });

            ThreadPool.WaitForIdle();
        }
    }
}
