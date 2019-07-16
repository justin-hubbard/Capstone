using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Skywalker;
using Skywalker.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Skywalker_TEST.Mapping {
    [TestClass]
    public class MapTest
    {
        private string cwd = Directory.GetCurrentDirectory();
        [TestMethod]
        public void ConvertDoesntCrash()
        {
            File.Delete(cwd + @"\mapping\mapping.json");
            try
            {
                Cartographer cart = new Cartographer();
                cart.Convert(cwd + @"\mapping");
            }
            catch (Exception e)
            {
                Assert.Fail("Should not crash: {0}", e.ToString());
            }
        }

        [TestMethod]
        public void ConvertsCorrectly()
        {
            File.Delete(cwd + @"\mapping\mapping.json");
            try
            {
                Cartographer cart = new Cartographer();
                cart.Convert(cwd + @"\mapping"); //converts and autosaves the converted map
                cart.Load(cwd + @"\mapping"); //loads the map

                List<string> rawmap = File.ReadAllLines(cwd + @"\mapping\map.csv").ToList();

                for (int y = 0; y < rawmap.Count(); y++) {
                    //break up line
                    List<string> valuesList = rawmap[y].Split(',').ToList();

                    for (int x = 0; x < valuesList.Count(); x++) {
                        //convert value to ushort, then create MapPoint
                        Assert.AreEqual(Convert.ToInt32(valuesList[x]), 
                            cart.GetPoint(x, y).GetOccupancy());
                    }
                }

            }
            catch (Exception e)
            {
               Assert.Fail("Should not crash: {0}", e.ToString());
            }
        }
    
        [TestMethod]
        public void LoadDoesntCrash()
        {
            try {
                Cartographer cart = new Cartographer();
                cart.Load(@"./");
            } catch (Exception e) {
                Assert.Fail("Should not crash: {0}", e.ToString());
            }
        }

        [TestMethod]
        public void LoadsCorrectly()
        {
            try
            {
                Cartographer cart = new Cartographer(cwd + @"\mapping");

                List<string> rawmap = File.ReadAllLines(cwd + @"\mapping\map.csv").ToList();

                for (int y = 0; y < rawmap.Count(); y++) {
                    //break up line
                    List<string> valuesList = rawmap[y].Split(',').ToList();

                    for (int x = 0; x < valuesList.Count(); x++) {
                        //convert value to ushort, then create MapPoint
                        Assert.AreEqual(Convert.ToInt32(valuesList[x]),
                            cart.GetPoint(x, y).GetOccupancy());
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Should not crash: {0}", e.ToString());
            }
        }

        [TestMethod]
        public void GetMapAttr()
        {
            try
            {
                Cartographer map = new Cartographer(cwd + @"\mapping");
                /*Cartographer map = new Cartographer();
                map.Convert(cwd + @"\mapping");
                map.Load(cwd + @"\mapping");*/
                string mapattrJSON = File.ReadAllText(cwd + @"\mapping\mapAttr.json");
                List<MapAttr> mapPoints = JsonConvert.DeserializeObject<List<MapAttr>>(mapattrJSON);

                for (var i = 0; i < mapPoints.Count; i++)
                {
                    var x = mapPoints[i].X;
                    var y = mapPoints[i].Y;
                    var mp = map.GetPoint(x, y);
                    var attr_index = mp.Attr_Index;
                    var attr = map.GetMapAttr(attr_index);
                    Assert.AreEqual(attr, mapPoints[i]);
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Should not crash: {0}", e.ToString());
            }
        }

        [TestMethod]
        public void GetPOI()
        {
            try {
                Cartographer map = new Cartographer(cwd + @"\mapping");
                /*Cartographer map = new Cartographer();
                map.Convert(cwd + @"\mapping");
                map.Load(cwd + @"\mapping");*/
                string poiJSON = File.ReadAllText(cwd + @"\mapping\poi.json");
                List<PointOfInterest> POIs = JsonConvert.DeserializeObject<List<PointOfInterest>>(poiJSON);

                for (var i = 0; i < POIs.Count; i++) {
                    var x = POIs[i].X;
                    var y = POIs[i].Y;
                    var poi_index = map.GetPoint(x, y).POI_Index;
                    var poi = map.GetPointOfInterest(poi_index);
                    Assert.AreEqual(poi, POIs[i]);
                }
            } catch (Exception e) {
                Assert.Fail("Should not crash: {0}", e.ToString());
            }
            
        }

        [TestMethod]
        public void CanGenerateBMP()
        {
            try {
                Cartographer map = new Cartographer(cwd + @"\mapping");
                WriteableBitmap mapBitmap = map.Render();
                using (FileStream stream5 = new FileStream(cwd + @"\mapping\map.png", FileMode.OpenOrCreate)) {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(mapBitmap));
                    encoder5.Save(stream5);
                    stream5.Close();
                }
            } catch (Exception e) {
                Assert.Fail("Should not crash: {0}", e.ToString());
            }
        }
    }
}
