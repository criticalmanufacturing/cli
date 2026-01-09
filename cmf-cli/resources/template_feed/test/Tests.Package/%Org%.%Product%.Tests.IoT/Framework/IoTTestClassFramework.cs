using cmConnect.TestFramework.ConnectIoT.Entities;
using cmConnect.TestFramework.Environment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Settings;
using System;
using System.Collections.Generic;

namespace AutomaticTests
{
    [TestClass]
    [DeploymentItem("Configuration.xml")]
    public abstract class TestClassFramework
    {
        #region Static Variables

        private static Dictionary<string, AutomationScenario> m_Scenarios = new Dictionary<string, AutomationScenario>();
        public static AutomationScenario m_Scenario;
        public static string m_mode = BaseContext.Mode.ToString();
        public static string m_FileNameRunSettings = BaseContext.FilePath;

        public IoTEnvironmentSpecific ioTEnvironmentSpecific = new IoTEnvironmentSpecific();

        public Persistency Persistency { get; internal set; }

        #endregion Static Variables

        public virtual void PrepareTestScenario(string equipmentOrCluster)
        {
            if (m_Scenarios.ContainsKey("Scenario"))
            {
                m_Scenario = m_Scenarios["Scenario"];
            }

            if (m_Scenario == null || m_Scenario.EquipmentToTest != equipmentOrCluster)
            {
                if (m_Scenario != null)
                {
                    // Terminate old scenario first...
                    foreach (KeyValuePair<string, cmConnect.TestFramework.Common.Interfaces.IEquipment> equipment in m_Scenario.Equipments)
                    {
                        try
                        {
                            equipment.Value.Terminate();
                        }
                        catch
                        {
                            //m_Log.Log("!!!! Error while terminating the equipment: {0}", e.Message);
                        }
                    }
                }

                Console.Write("\r\n{0}: Running Tests for '{1}': .", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"), equipmentOrCluster);

                m_Scenario = new AutomationScenario(ioTEnvironmentSpecific, equipmentOrCluster, m_mode,
                    pathToConfigurationFile: m_FileNameRunSettings);

                m_Scenarios["Scenario"] = m_Scenario;
            }
        }

        public virtual void AfterTest()
        {
            try
            {
                foreach (KeyValuePair<string, cmConnect.TestFramework.Common.Interfaces.IEquipment> equipment in m_Scenario.Equipments)
                {
                    // Clear all variables values
                    equipment.Value.Variables.Clear();
                }

                Cleanup();
            }
            catch
            {
            }
        }

        /// <summary>Terminate the last test and close the cmConnect</summary>
        public static void Cleanup()
        {
            if (m_Scenario != null)
            {
                // Terminate old scenario first...
                foreach (KeyValuePair<string, cmConnect.TestFramework.Common.Interfaces.IEquipment> equipment in m_Scenario.Equipments)
                {
                    try
                    {
                        equipment.Value.Terminate();
                    }
                    catch
                    {
                        //m_Log.Log("!!!! Error while terminating the equipment: {0}", e.Message);
                    }
                }
            }
        }
    }
}