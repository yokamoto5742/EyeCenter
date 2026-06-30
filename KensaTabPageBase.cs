using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MedicalLibrary.Agent;

namespace EyeCenter
{
    class KensaTabPageBase : TabPage
    {
        protected string KensaId = "";

        protected DataRow KensaPageRow;

        protected virtual void ControlShow(string kensa_id)
        {
        }

        public virtual void KensaShow(EyeKensa kensa)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kensa"></param>
        /// <param name="count">åüç∏ÉfÅ[É^ÇÃêî</param>
        public virtual void KensaShow(EyeKensa2 kensa, int count)
        {
        }

        public virtual void KensaClear()
        {
        }
    }
}
