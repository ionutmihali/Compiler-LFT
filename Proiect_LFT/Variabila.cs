using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_LFT
{
    internal class Variabila
    {
        private int type;
        private string name;
        private object val;

        public Variabila()
        {
            this.type = 0;
            this.name= string.Empty;
            this.val = null;
        }
        public Variabila(int type, string name, dynamic val)
        {
            this.type = type;
            this.name = name;
            this.val = val;
        }

        public void setType(int type)
        {
            this.type = type;
        }
        public void setName(string name) 
        { 
            this.name = name;
        }

        public void setVal(dynamic val) 
        {   
            this.val = val;
        }

        public object getVal() 
        { 
            return this.val;
        }

        public string getName()
        {
            return this.name;
        }

        public int getType() 
        { 
            return this.type;
        }
    }
}
