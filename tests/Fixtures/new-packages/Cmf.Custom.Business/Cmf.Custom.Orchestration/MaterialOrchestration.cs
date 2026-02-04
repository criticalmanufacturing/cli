using System.Data;
using System.Collections.Generic;

namespace Cmf.Custom.Orchestration
{
    /// <summary>
    /// Example class demonstrating Load() calls in foreach loops (anti-pattern)
    /// This file is used for testing the Business Package Linter
    /// </summary>
    public class MaterialOrchestration
    {
        private readonly dynamic _entityFactory;

        public MaterialOrchestration(dynamic entityFactory)
        {
            _entityFactory = entityFactory;
        }

        /// <summary>
        /// BAD PRACTICE: This method calls Load() inside a foreach loop
        /// </summary>
        public List<dynamic> GetMaterialsWithLoadInLoop(DataTable results)
        {
            var materials = new List<dynamic>();
            
            foreach (DataRow dataRow in results.Rows)
            {
                dynamic material = _entityFactory.Create("Material");
                material.Name = (string)dataRow["Name"];
                material.Load();            // BAD: Load() inside foreach
                material.Facility.Load();  // BAD: Load() inside foreach
                materials.Add(material);
            }
            
            return materials;
        }

        /// <summary>
        /// GOOD PRACTICE: This method collects objects first, then loads them
        /// </summary>
        public List<dynamic> GetMaterialsCorrectly(DataTable results)
        {
            var materials = new List<dynamic>();
            
            foreach (DataRow dataRow in results.Rows)
            {
                dynamic material = _entityFactory.Create("Material");
                material.Name = (string)dataRow["Name"];
                materials.Add(material);
            }
            
            // Load all materials at once (collection load)
            // materials.LoadCollection();
            
            return materials;
        }

        /// <summary>
        /// ACCEPTABLE: Load() is called outside the loop
        /// </summary>
        public dynamic GetSingleMaterial(string name)
        {
            dynamic material = _entityFactory.Create("Material");
            material.Name = name;
            material.Load();  // OK: Not inside a loop
            return material;
        }
    }
}
