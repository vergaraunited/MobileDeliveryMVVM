using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.Models
{
    public class OrderOptionsModel : INotifyPropertyChanged
    {
        public int LineNumber { get; set; }
        public long ORD_NO { get; set; }
        public short MODEL { get; set; }
        public short MDL_NO { get; set; }
        public long DLR_NO { get; set; }
        public short OPT_NUM { get; set; }
        public string DESC { get; set; }
        public string CLR { get; set; }
        public short MDL_CNT { get; set; }

        public OrderOptionsModel(orderOptions mst)
        {
            ORD_NO = BitConverter.ToInt64(mst.ORD_NO, 0);
            MDL_NO = BitConverter.ToInt16(mst.MDL_NO, 0);

            MDL_NO = BitConverter.ToInt16(mst.MODEL, 0);
            OPT_NUM = BitConverter.ToInt16(mst.OPT_NUM, 0);
            DESC = BitConverter.ToString(mst.DESC, 0, fldsz_DESCOrd);
            CLR = BitConverter.ToString(mst.CLR, 0, fldsz_CLR);
            
            MDL_CNT = BitConverter.ToInt16(mst.MDL_CNT, 0);
           
        }
        public OrderOptionsModel(orderMaster mst)
        {
            // ORD_NO = mst.ORD_NO;
            //DLR_NO = mst.DLR_NO;
            //OPT_NUM = BitConverter.ToInt16(mst.OPT_NUM, 0);
            //DESC = BitConverter.ToString(mst.DESC, 0, fldsz_DESCOrd);
            //CLR = BitConverter.ToString(mst.CLR, 0, fldsz_CLR);
            //WIDTH = BitConverter.ToInt64(mst.WIDTH, 0);
            //HEIGHT = BitConverter.ToInt64(mst.HEIGHT, 0);
            //MDL_CNT = BitConverter.ToInt16(mst.MDL_CNT, 0);
            //WIN_CNT = BitConverter.ToInt16(mst.WIN_CNT, 0);
            //RTE_CDE = mst.RTE_CDE;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        //z List<Item> itList = new List<Item>();

    }
}
