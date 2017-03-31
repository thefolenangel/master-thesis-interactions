using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebDataParser;

namespace WebDataParser.Models {
    public class TestDataViewModel {
        public string ID { get; }

        public Dictionary<string, GestureInfo> GestureInformation { get; set; }

        public TestDataViewModel(string id) {
            ID = id;
            GestureInformation = new Dictionary<string, GestureInfo>();
        }
    }
}