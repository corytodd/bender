namespace Bender.Core.Nodes
{
    using System;
    using System.IO;
    using Layouts;

    /// <summary>
    ///     Error specialization capture node errors
    ///     stemming from a discrepancy between the
    ///     spec file and the target binary.
    /// </summary>
    public class BError : BaseNode
    {
        /// <summary>
        ///     Create a new error
        /// </summary>
        /// <param name="error">Short error name</param>
        /// <param name="details">Explanation and context</param>
        /// <param name="ex">Associated exception, if any</param>
        public BError(string error, string details, Exception ex = null) : this(null, error, details, ex)
        {
        }
        
        /// <summary>
        ///     Create a new error
        /// </summary>
        /// <param name="el">Parent</param>
        /// <param name="error">Short error name</param>
        /// <param name="details">Explanation and context</param>
        /// <param name="ex">Associated exception, if any</param>
        public BError(Element el, string error, string details, Exception ex = null) : base(el)
        {
            Error = error;
            Details = details;
            Exception = ex;
        }

        /// <summary>
        ///     Runtime exception that generated this error
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     Short error code
        /// </summary>
        public string Error { get; }

        /// <summary>
        ///     Explanation and context of the error
        /// </summary>
        public string Details { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} : {Error}|{Details}";
        }

        /// <inheritdoc />
        public override void Render(StreamWriter stream)
        {
            stream.Write(this);
        }
    }
}