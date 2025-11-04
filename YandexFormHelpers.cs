using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckPosition
{
    internal class YandexFormHelpers
    {
    }

    class JsonDimensionName
    {
        public string name;
    }
    class JsonDimensions
    {
        public JsonDimensionName[] dimensions;
        public float[] metrics;
    }
    class JsonResponse
    {

        public object query;
        public List<JsonDimensions> data;

        public int total_rows;
        public bool total_rows_rounded;
        public bool sampled;
        public bool contains_sensitive_data;
        public float sample_share;
        public int sample_size;
        public int sample_space;
        public int data_lag;
        public float[] totals;
        public float[] min;
        public float[] max;
        public float google
        {
            get
            {
                float cnt = 0;
                foreach (JsonDimensions item in data)
                {
                    if (item.dimensions.Length > 0 && item.dimensions[0].name.ToLower().IndexOf("google") >= 0)
                        cnt += item.metrics[0];
                }
                return cnt;
            }
        }
        public float yandex
        {
            get
            {
                float cnt = 0;
                foreach (JsonDimensions item in data)
                {
                    if (item.dimensions.Length > 0 && item.dimensions[0].name.ToLower().IndexOf("yandex") >= 0)
                        cnt += item.metrics[0];
                }
                return cnt;
            }
        }

        public float total
        {
            get
            {
                return this.totals.Length > 0 ? this.totals[0] : 0;
            }
        }

        public float other
        {
            get
            {
                return this.total - this.yandex - this.google;
            }
        }
    }
}
