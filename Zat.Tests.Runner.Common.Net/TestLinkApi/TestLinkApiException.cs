/*
TestLink API library
Copyright (c) 2009, Stephan Meyn <stephanmeyn@gmail.com>

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.
*/

namespace Zat.Tests.Runner.Common.Net.TestLinkApi;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// basic exception thrown whenever Testlink returns an error
/// </summary>
[Serializable]
public class TestLinkApiException : ApplicationException
{
    /// <summary>
    /// temporarily stores current errors
    /// </summary>
    public List<Model.TestLinkErrorMessage> Errors;

    /// <summary>
    /// basic Constructor
    /// </summary>
    public TestLinkApiException() : base("Zat.Tests.Runner.TestLinkApi.TestLinkApiException: testlink returned error messages.")
    {
    }

    /// <summary>
    /// vConstructor that can take an inner exception
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="innerException"></param>
    public TestLinkApiException(string msg, Exception innerException) : base(msg, innerException)
    {
    }

    protected TestLinkApiException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <summary>
    /// constructor that takes a list of error messages
    /// </summary>
    /// <param name="errs"></param>
    public TestLinkApiException(List<Model.TestLinkErrorMessage> errs)
        : base("TestLinkApiException: testlink returned error messages. See errors")
    {
        this.Errors = errs;
        foreach (var error in errs)
            this.Data.Add(error.Code, error.Message);
    }

    /// <summary>
    /// constructor 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="errs"></param>
    public TestLinkApiException(string msg, List<Model.TestLinkErrorMessage> errs)
        : base(msg)
    {
        this.Errors = errs;
        foreach (var error in errs)
            this.Data.Add(error.Code, error.Message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    public TestLinkApiException(string msg) : base(msg)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fmt"></param>
    /// <param name="args"></param>
    public TestLinkApiException(string fmt, params object[] args) : base(string.Format(fmt, args))
    {
    }

    /// <inheritdoc/>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }
}