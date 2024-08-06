using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BK.Util
{
    /// <summary>
    /// The class BK.Util.MultiMap always throws one exception type - MultiMapBKException
    /// </summary>
    public class MultiMapBKException: Exception
    {
        /// <summary>
        /// Constructor of Exception class
        /// </summary>
        /// <param name="ExceptionParam"></param>
        /// <param name="ExMessage"></param>
        /// 
        public MultiMapBKException(Exception ExceptionParam, string ExMessage) : base(ExMessage, ExceptionParam) 
        {
        }

        /// <summary>
        /// Default Constructor of Exception class
        /// </summary>
        public MultiMapBKException():base()
        {
        }

        /// <summary>
        ///  Constructor of Exception class
        /// </summary>
        /// <param name="exMessage"></param>
        public MultiMapBKException(string exMessage):base(exMessage)
        {

        }

        /// <summary>
        /// Multimap Exception constructor
        /// </summary>
        /// <param name="seInfo"></param>
        /// <param name="seCtxt"></param>
        public MultiMapBKException(System.Runtime.Serialization.SerializationInfo seInfo,
            System.Runtime.Serialization.StreamingContext seCtxt)
            : base(seInfo, seCtxt)
        {
        }
            


    }
}
