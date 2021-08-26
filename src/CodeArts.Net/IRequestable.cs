using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeArts.Net
{
    /// <summary>
    /// 请求基础。
    /// </summary>
    public interface IRequestableBase
    {
        /// <summary>
        /// 指定包含与请求或响应相关联的协议头。
        /// </summary>
        /// <param name="header">协议头。</param>
        /// <param name="value">内容。</param>
        /// <returns></returns>
        IRequestable AssignHeader(string header, string value);

        /// <summary>
        /// 指定包含与请求或响应相关联的协议头。
        /// </summary>
        /// <param name="headers">协议头。</param>
        /// <returns></returns>
        IRequestable AssignHeaders(IEnumerable<KeyValuePair<string, string>> headers);

        /// <summary>
        /// 请求参数。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <returns></returns>
        IRequestable AppendQueryString(string param);

        /// <summary>
        /// 请求参数。?id=1&amp;name="yep"。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="value">参数值。</param>
        /// <returns></returns>
        IRequestable AppendQueryString(string name, string value);

        /// <summary>
        /// 请求参数。例如：?id=1&amp;name="yep"。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="value">参数值。</param>
        /// <param name="dateFormatString">日期格式化。</param>
        /// <returns></returns>
        IRequestable AppendQueryString(string name, DateTime value, string dateFormatString = "yyyy-MM-dd HH:mm:ss.FFFFFFFK");

        /// <summary>
        /// 请求参数。?id=1&amp;name="yep"。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="value">参数值。</param>
        /// <returns></returns>
        IRequestable AppendQueryString<T>(string name, T value);

        /// <summary>
        /// 请求参数。?id=1&amp;name="yep"。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <returns></returns>
        IRequestable AppendQueryString(IEnumerable<string> param);

        /// <summary>
        /// 请求参数。?id=1&amp;name="yep"。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <returns></returns>
        IRequestable AppendQueryString(IEnumerable<KeyValuePair<string, string>> param);

        /// <summary>
        /// 请求参数。?id=1&amp;name="yep"。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <param name="dateFormatString">日期格式化。</param>
        /// <returns></returns>
        IRequestable AppendQueryString(IEnumerable<KeyValuePair<string, DateTime>> param, string dateFormatString = "yyyy-MM-dd HH:mm:ss.FFFFFFFK");

        /// <summary>
        /// 请求参数。?id=1&amp;name="yep"。
        /// </summary>
        /// <typeparam name="T">值类型。</typeparam>
        /// <param name="param">参数。</param>
        /// <returns></returns>
        IRequestable AppendQueryString<T>(IEnumerable<KeyValuePair<string, T>> param);

        /// <summary>
        /// 请求参数。?id=1&amp;name="yep"。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <param name="namingType">命名规则。</param>
        /// <param name="dateFormatString">日期格式化。</param>
        /// <returns></returns>
        IRequestable AppendQueryString(object param, NamingType namingType = NamingType.UrlCase, string dateFormatString = "yyyy-MM-dd HH:mm:ss.FFFFFFFK");
    }

    /// <summary>
    /// 请求。
    /// </summary>
    public interface IRequestable : IRequestableExtend<string>, IRequestableBase, IFileRequestable, ICastRequestable
    {
        /// <summary>
        /// 使用编码，默认:UTF-8。
        /// </summary>
        /// <param name="encoding">编码。</param>
        IRequestable UseEncoding(Encoding encoding);

        /// <summary>
        /// body中传输。
        /// </summary>
        /// <param name="body">body内容。</param>
        /// <param name="contentType">Content-Type类型。</param>
        /// <returns></returns>
        IRequestable Body(string body, string contentType);

        /// <summary>
        /// content-type = "application/json"。
        /// </summary>
        /// <param name="json">参数。</param>
        /// <returns></returns>
        IRequestable Json(string json);
        /// <summary>
        ///  content-type = "application/json"。
        /// </summary>
        /// <typeparam name="T">参数类型。</typeparam>
        /// <param name="param">参数。</param>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IRequestable Json<T>(T param, NamingType namingType = NamingType.Normal) where T : class;
        /// <summary>
        /// content-type = "application/xml";。
        /// </summary>
        /// <param name="xml">参数。</param>
        /// <returns></returns>
        IRequestable Xml(string xml);
        /// <summary>
        /// content-type = "application/xml";。
        /// </summary>
        /// <typeparam name="T">参数类型。</typeparam>
        /// <param name="param">参数。</param>
        /// <returns></returns>
        IRequestable Xml<T>(T param) where T : class;
        /// <summary>
        /// content-type = "application/x-www-form-urlencoded";。
        /// </summary>
        /// <param name="json">JSON格式的参数。</param>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IRequestable Form(string json, NamingType namingType = NamingType.Normal);
        /// <summary>
        /// content-type = "application/x-www-form-urlencoded";。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IRequestable Form(IEnumerable<KeyValuePair<string, string>> param, NamingType namingType = NamingType.Normal);
        /// <summary>
        /// content-type = "application/x-www-form-urlencoded";。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IRequestable Form(IEnumerable<KeyValuePair<string, DateTime>> param, NamingType namingType = NamingType.Normal);
        /// <summary>
        /// content-type = "application/x-www-form-urlencoded";。
        /// </summary>
        /// <typeparam name="T">值类型。</typeparam>
        /// <param name="param">参数。</param>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IRequestable Form<T>(IEnumerable<KeyValuePair<string, T>> param, NamingType namingType = NamingType.Normal);
        /// <summary>
        /// 属性名称按照<see cref="NamingAttribute"/>标记分析。
        /// content-type = "application/x-www-form-urlencoded";。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <returns></returns>
        IRequestable Form(object param);
        /// <summary>
        /// content-type = "application/x-www-form-urlencoded";。
        /// </summary>
        /// <param name="param">参数。</param>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IRequestable Form(object param, NamingType namingType);
        /// <summary>
        /// 如果请求异常，会调用【<paramref name="match"/>】，判断是否重试请求。
        /// 多个条件是或的关系。
        /// </summary>
        /// <param name="match">判断是否重试请求。</param>
        /// <returns></returns>
        IThenRequestable TryIf(Predicate<WebException> match);

        /// <summary>
        /// 如果请求异常，会调用【<paramref name="then"/>】，并重试一次请求。
        /// </summary>
        /// <param name="then">异常处理事件。</param>
        /// <returns></returns>
        IThenConditionRequestable TryThen(Action<IRequestableBase, WebException> then);

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
        /// <summary>
        /// 如果请求异常，会调用【<paramref name="then"/>】，并重试一次请求。
        /// </summary>
        /// <param name="then">异常处理事件。</param>
        /// <returns></returns>
        IThenConditionRequestableAsync TryThenAsync(Func<IRequestableBase, WebException, Task> then);
#endif

        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        ICatchRequestable WebCatch(Action<WebException> log);
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultStringCatchRequestable WebCatch(Func<WebException, string> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestable Finally(Action log);
    }

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
    /// <summary>
    /// 异步请求能力。
    /// </summary>
    public interface IRequestableAsync<T>
    {
        /// <summary>
        /// GET 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        Task<T> GetAsync(int timeout = 5000);

        /// <summary>
        /// DELETE 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        Task<T> DeleteAsync(int timeout = 5000);

        /// <summary>
        /// POST 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        Task<T> PostAsync(int timeout = 5000);

        /// <summary>
        /// POST 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        Task<T> PutAsync(int timeout = 5000);

        /// <summary>
        /// HEAD 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        Task<T> HeadAsync(int timeout = 5000);

        /// <summary>
        /// PATCH 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        Task<T> PatchAsync(int timeout = 5000);

        /// <summary>
        /// 数据返回XML格式的结果，将转为指定类型。
        /// </summary>
        /// <param name="method">求取方式。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        Task<T> RequestAsync(string method, int timeout = 5000);
    }

    /// <summary>
    /// 文件异步下载。
    /// </summary>
    public interface IFileRequestableAsync
    {
        /// <summary>
        /// 上传文件。
        /// </summary>
        /// <param name="fileName">本地文件地址：指定文件将被上传到服务地址。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        Task<byte[]> UploadFileAsync(string fileName, int timeout = 5000);

        /// <summary>
        /// 上传文件。
        /// </summary>
        /// <param name="method">求取方式。</param>
        /// <param name="fileName">本地文件地址：指定文件将被上传到服务地址。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        Task<byte[]> UploadFileAsync(string method, string fileName, int timeout = 5000);

        /// <summary>
        /// 下载文件。
        /// </summary>
        /// <param name="fileName">本地文件地址：文件将下载到这个路径下。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        Task DownloadFileAsync(string fileName, int timeout = 5000);
    }

    /// <summary>
    /// 异步重试迭代请求能力扩展。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResendIntervalVerifyRequestableExtendAsync<T> : IRequestableAsync<T>
    {
    }

    /// <summary>
    /// 异步重试请求能力。
    /// </summary>
    public interface IResendVerifyRequestableExtendAsync<T> : IRequestableAsync<T>
    {
        /// <summary>
        /// 失败后，间隔<paramref name="millisecondsTimeout"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="millisecondsTimeout">失败后，间隔多久重试。单位：毫秒。</param>
        /// <returns></returns>
        IResendIntervalVerifyRequestableExtendAsync<T> ResendInterval(int millisecondsTimeout);

        /// <summary>
        /// 失败后，间隔<paramref name="interval"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="interval">第一个参数：异常，第二个参数：第N次重试，返回间隔多少时间重试请求。单位：毫秒。</param>
        /// <returns></returns>
        IResendIntervalVerifyRequestableExtendAsync<T> ResendInterval(Func<T, int, int> interval);
    }

    /// <summary>
    /// 延续能力的请求。
    /// </summary>
    public interface IVerifyRequestableExtendAsync<T> : IRequestableAsync<T>
    {
        /// <summary>
        /// 结果请求结果不满足<paramref name="predicate"/>时，会重复请求。
        /// 多个条件之间是且的关系。
        /// </summary>
        /// <param name="predicate">判断是否重试请求。</param>
        /// <returns></returns>
        IVerifyRequestableExtendAsync<T> And(Predicate<T> predicate);

        /// <summary>
        /// 设置重试次数。
        /// </summary>
        /// <param name="retryCount">最大重试次数。</param>
        /// <returns></returns>
        IResendVerifyRequestableExtendAsync<T> ResendCount(int retryCount);
    }

    /// <summary>
    /// 异步请求能力扩展。
    /// </summary>
    public interface IRequestableExtendAsync<T> : IRequestableAsync<T>
    {
        /// <summary>
        /// 结果请求结果不满足<paramref name="predicate"/>时，会重复请求。
        /// </summary>
        /// <param name="predicate">结果验证函数。</param>
        /// <returns></returns>
        IVerifyRequestableExtendAsync<T> DataVerify(Predicate<T> predicate);
    }

    /// <summary>
    /// 异步异常处理能力。
    /// </summary>
    public interface IResultCatchRequestableAsync<T> : IRequestableExtendAsync<T>
    {
        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestableAsync<T> Finally(Action log);
    }

    /// <summary>
    /// 异步异常处理能力。
    /// </summary>
    public interface IJsonResultCatchRequestableAsync<T> : IResultCatchRequestableAsync<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestableAsync<T> WebCatch(Func<WebException, T> returnValue);
    }

    /// <summary>
    /// 异步异常处理能力。
    /// </summary>
    public interface IXmlResultCatchRequestableAsync<T> : IResultCatchRequestableAsync<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestableAsync<T> WebCatch(Func<WebException, T> returnValue);
    }

    /// <summary>
    /// 异步结束。
    /// </summary>
    public interface IFinallyRequestableAsync<T> : IRequestableExtendAsync<T>
    {
    }

    /// <summary>
    /// 异步异常处理能力。
    /// </summary>
    public interface IXmlCatchRequestableAsync<T> : IRequestableExtendAsync<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestableAsync<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IXmlCatchRequestableAsync<T> XmlCatch(Action<string, XmlException> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IXmlResultCatchRequestableAsync<T> XmlCatch(Func<string, XmlException, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestableAsync<T> Finally(Action log);
    }

    /// <summary>
    /// 异步异常处理能力。
    /// </summary>
    public interface IJsonCatchRequestableAsync<T> : IRequestableExtendAsync<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestableAsync<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IJsonCatchRequestableAsync<T> JsonCatch(Action<string, Exception> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IJsonResultCatchRequestableAsync<T> JsonCatch(Func<string, Exception, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestableAsync<T> Finally(Action log);
    }

    /// <summary>
    /// 异步请求能力。
    /// </summary>
    /// <typeparam name="T">结果数据。</typeparam>
    public interface IJsonRequestableAsync<T> : IRequestableExtendAsync<T>
    {
        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IJsonResultCatchRequestableAsync<T> JsonCatch(Action<string, Exception> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IJsonResultCatchRequestableAsync<T> JsonCatch(Func<string, Exception, T> returnValue);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestableAsync<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestableAsync<T> Finally(Action log);
    }

    /// <summary>
    /// 异步请求能力。
    /// </summary>
    /// <typeparam name="T">结果数据。</typeparam>
    public interface IXmlRequestableAsync<T> : IRequestableExtendAsync<T>
    {
        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IXmlCatchRequestableAsync<T> XmlCatch(Action<string, XmlException> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IXmlResultCatchRequestableAsync<T> XmlCatch(Func<string, XmlException, T> returnValue);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestableAsync<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestableAsync<T> Finally(Action log);
    }

    /// <summary>
    /// 异步转换能力。
    /// </summary>
    public interface ICastRequestableAsync
    {
        /// <summary>
        /// 数据返回JSON格式的结果，将转为指定类型。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IJsonRequestableAsync<T> JsonCast<T>(NamingType namingType = NamingType.Normal) where T : class;
        /// <summary>
        /// 数据返回XML格式的结果，将转为指定类型。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <returns></returns>
        IXmlRequestableAsync<T> XmlCast<T>() where T : class;

        /// <summary>
        /// 数据返回JSON格式的结果，将转为指定类型(匿名对象)。
        /// </summary>
        /// <typeparam name="T">匿名类型。</typeparam>
        /// <param name="anonymousTypeObject">匿名对象。</param>
        /// <param name="namingType">命名规范。</param>
        /// <returns></returns>
        IJsonRequestableAsync<T> JsonCast<T>(T anonymousTypeObject, NamingType namingType = NamingType.Normal) where T : class;
        /// <summary>
        /// 数据返回XML格式的结果，将转为指定类型(匿名对象)。
        /// </summary>
        /// <typeparam name="T">匿名类型。</typeparam>
        /// <param name="anonymousTypeObject">匿名对象。</param>
        /// <returns></returns>
        IXmlRequestableAsync<T> XmlCast<T>(T anonymousTypeObject) where T : class;
    }

    /// <summary>
    /// 异步字符串结束。
    /// </summary>
    public interface IFinallyStringRequestableAsync : IRequestableExtendAsync<string>
    {
    }

    /// <summary>
    /// 异步字符串结果。
    /// </summary>
    public interface IResultStringCatchRequestableAsync : IRequestableExtendAsync<string>
    {
        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyStringRequestableAsync Finally(Action log);
    }

    /// <summary>
    /// 异步结束。
    /// </summary>
    public interface IFinallyRequestableAsync : IRequestableExtendAsync<string>, ICastRequestableAsync, IFileRequestableAsync
    {
    }

    /// <summary>
    /// 异常处理的请求。
    /// </summary>
    public interface ICatchRequestableAsync : IRequestableExtendAsync<string>, ICastRequestableAsync, IFileRequestableAsync
    {
        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        ICatchRequestableAsync WebCatch(Action<WebException> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultStringCatchRequestableAsync WebCatch(Func<WebException, string> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestableAsync Finally(Action log);
    }

    /// <summary>
    /// 间隔时间重试。
    /// </summary>
    public interface IRetryIntervalThenRequestableAsync : ICatchRequestableAsync, IFileRequestableAsync
    {
    }

    /// <summary>
    /// 最大重试次数设置。
    /// </summary>
    public interface IRetryThenRequestableAsync : ICatchRequestableAsync, IFileRequestableAsync
    {
        /// <summary>
        /// 失败后，间隔<paramref name="millisecondsTimeout"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="millisecondsTimeout">失败后，间隔多久重试。单位：毫秒。</param>
        /// <returns></returns>
        IRetryIntervalThenRequestableAsync RetryInterval(int millisecondsTimeout);

        /// <summary>
        /// 失败后，间隔<paramref name="interval"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="interval">第一个参数：异常，第二个参数：第N次重试，返回间隔多少时间重试请求。单位：毫秒。</param>
        /// <returns></returns>
        IRetryIntervalThenRequestableAsync RetryInterval(Func<WebException, int, int> interval);
    }

    /// <summary>
    /// 异步延续能力的请求。
    /// </summary>
    public interface IThenRequestableAsync : ICatchRequestableAsync, IFileRequestableAsync
    {
        /// <summary>
        /// 如果请求异常，会调用【<paramref name="predicate"/>】，判断是否重试请求。
        /// 多个条件之间是或的关系。
        /// </summary>
        /// <param name="predicate">判断是否重试请求。</param>
        /// <returns></returns>
        IThenRequestableAsync Or(Predicate<WebException> predicate);

        /// <summary>
        /// 设置重试次数。
        /// </summary>
        /// <param name="retryCount">最大重试次数。</param>
        /// <returns></returns>
        IRetryThenRequestableAsync RetryCount(int retryCount);
    }

    /// <summary>
    /// 带有条件的延续能力请求。
    /// </summary>
    public interface IThenAndConditionRequestableAsync : ICatchRequestableAsync, IFileRequestableAsync
    {
        /// <summary>
        /// 所有条件都满足时，重试一次请求。
        /// 多个条件之间是且的关系。
        /// </summary>
        /// <param name="predicate">条件。</param>
        /// <returns></returns>
        IThenAndConditionRequestableAsync And(Predicate<WebException> predicate);

        /// <summary>
        /// 如果请求异常，会调用【<paramref name="match"/>】，判断是否重试请求。
        /// 多个条件是或的关系。
        /// </summary>
        /// <param name="match">判断是否重试请求。</param>
        /// <returns></returns>
        IThenRequestableAsync TryIf(Predicate<WebException> match);

        /// <summary>
        /// 新开一个重试机制，如果请求异常，会调用【<paramref name="thenAsync"/>】，并重试一次请求。
        /// </summary>
        /// <param name="thenAsync">异常处理事件。</param>
        /// <returns></returns>
        IThenConditionRequestableAsync ThenAsync(Func<IRequestableBase, WebException, Task> thenAsync);
    }

    /// <summary>
    /// 带有条件的异步延续能力请求。
    /// </summary>
    public interface IThenConditionRequestableAsync
    {
        /// <summary>
        /// 所有条件都满足时，重试一次请求。
        /// 多个条件之间是且的关系。
        /// </summary>
        /// <param name="predicate">条件。</param>
        /// <returns></returns>
        IThenAndConditionRequestableAsync If(Predicate<WebException> predicate);
    }
#endif

    /// <summary>
    /// 请求能力。
    /// </summary>
#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
    public interface IRequestable<T> : IRequestableAsync<T>
#else
    public interface IRequestable<T>
#endif
    {
        /// <summary>
        /// GET 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        T Get(int timeout = 5000);

        /// <summary>
        /// DELETE 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        T Delete(int timeout = 5000);

        /// <summary>
        /// POST 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        T Post(int timeout = 5000);

        /// <summary>
        /// POST 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        T Put(int timeout = 5000);

        /// <summary>
        /// HEAD 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        T Head(int timeout = 5000);

        /// <summary>
        /// PATCH 请求。
        /// </summary>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        T Patch(int timeout = 5000);

        /// <summary>
        /// 数据返回XML格式的结果，将转为指定类型。
        /// </summary>
        /// <param name="method">求取方式。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        /// <returns></returns>
        T Request(string method, int timeout = 5000);
    }

    /// <summary>
    /// 文件下载。
    /// </summary>
#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
    public interface IFileRequestable : IFileRequestableAsync
#else
    public interface IFileRequestable
#endif
    {
        /// <summary>
        /// 上传文件。
        /// </summary>
        /// <param name="fileName">本地文件地址：指定文件将被上传到服务地址。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        byte[] UploadFile(string fileName, int timeout = 5000);

        /// <summary>
        /// 上传文件。
        /// </summary>
        /// <param name="method">求取方式。</param>
        /// <param name="fileName">本地文件地址：指定文件将被上传到服务地址。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        byte[] UploadFile(string method, string fileName, int timeout = 5000);

        /// <summary>
        /// 下载文件。
        /// </summary>
        /// <param name="fileName">本地文件地址：文件将下载到这个路径下。</param>
        /// <param name="timeout">超时时间，单位：毫秒。</param>
        void DownloadFile(string fileName, int timeout = 5000);
    }

    /// <summary>
    /// 重试迭代请求能力扩展。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResendIntervalVerifyRequestableExtend<T> : IRequestable<T>
    {
    }

    /// <summary>
    /// 重试请求能力。
    /// </summary>
    public interface IResendVerifyRequestableExtend<T> : IRequestable<T>
    {
#if NET40
        /// <summary>
        /// 失败后，间隔<paramref name="millisecondsTimeout"/>毫秒后重试请求<see cref="System.Threading.Thread.Sleep(int)"/>。
        /// </summary>
        /// <param name="millisecondsTimeout">失败后，间隔多久重试。单位：毫秒。</param>
        /// <returns></returns>
#else
        /// <summary>
        /// 失败后，间隔<paramref name="millisecondsTimeout"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="millisecondsTimeout">失败后，间隔多久重试。单位：毫秒。</param>
        /// <returns></returns>
#endif
        IResendIntervalVerifyRequestableExtend<T> ResendInterval(int millisecondsTimeout);

#if NET40
        /// <summary>
        /// 失败后，间隔<paramref name="interval"/>毫秒后重试请求<see cref="System.Threading.Thread.Sleep(int)"/>。
        /// </summary>
        /// <param name="interval">第一个参数：异常，第二个参数：第N次重试，返回间隔多少时间重试请求。单位：毫秒。</param>
        /// <returns></returns>
#else
        /// <summary>
        /// 失败后，间隔<paramref name="interval"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="interval">第一个参数：异常，第二个参数：第N次重试，返回间隔多少时间重试请求。单位：毫秒。</param>
        /// <returns></returns>
#endif
        IResendIntervalVerifyRequestableExtend<T> ResendInterval(Func<T, int, int> interval);
    }

    /// <summary>
    /// 延续能力的请求。
    /// </summary>
    public interface IVerifyRequestableExtend<T> : IRequestable<T>
    {
        /// <summary>
        /// 结果请求结果不满足<paramref name="predicate"/>时，会重复请求。
        /// 多个条件之间是且的关系。
        /// </summary>
        /// <param name="predicate">判断是否重试请求。</param>
        /// <returns></returns>
        IVerifyRequestableExtend<T> And(Predicate<T> predicate);

        /// <summary>
        /// 设置重试次数。
        /// </summary>
        /// <param name="retryCount">最大重试次数。</param>
        /// <returns></returns>
        IResendVerifyRequestableExtend<T> ResendCount(int retryCount);
    }

    /// <summary>
    /// 请求能力扩展。
    /// </summary>
    public interface IRequestableExtend<T> : IRequestable<T>
    {
        /// <summary>
        /// 结果请求结果不满足<paramref name="predicate"/>时，会重复请求。
        /// </summary>
        /// <param name="predicate">结果验证函数。</param>
        /// <returns></returns>
        IVerifyRequestableExtend<T> DataVerify(Predicate<T> predicate);
    }

    /// <summary>
    /// 异常处理能力。
    /// </summary>
    public interface IResultCatchRequestable<T> : IRequestableExtend<T>
    {
        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestable<T> Finally(Action log);
    }

    /// <summary>
    /// 异常处理能力。
    /// </summary>
    public interface IJsonResultCatchRequestable<T> : IResultCatchRequestable<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestable<T> WebCatch(Func<WebException, T> returnValue);
    }

    /// <summary>
    /// 异常处理能力。
    /// </summary>
    public interface IXmlResultCatchRequestable<T> : IResultCatchRequestable<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestable<T> WebCatch(Func<WebException, T> returnValue);
    }

    /// <summary>
    /// 异常处理能力。
    /// </summary>
    public interface IJsonCatchRequestable<T> : IRequestableExtend<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestable<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IJsonCatchRequestable<T> JsonCatch(Action<string, Exception> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IJsonResultCatchRequestable<T> JsonCatch(Func<string, Exception, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestable<T> Finally(Action log);
    }

    /// <summary>
    /// 异常处理能力。
    /// </summary>
    public interface IXmlCatchRequestable<T> : IRequestableExtend<T>
    {
        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestable<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IXmlCatchRequestable<T> XmlCatch(Action<string, XmlException> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IXmlResultCatchRequestable<T> XmlCatch(Func<string, XmlException, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestable<T> Finally(Action log);
    }

    /// <summary>
    /// 请求能力。
    /// </summary>
    /// <typeparam name="T">结果数据。</typeparam>
    public interface IJsonRequestable<T> : IRequestableExtend<T>
    {
        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IJsonCatchRequestable<T> JsonCatch(Action<string, Exception> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IJsonResultCatchRequestable<T> JsonCatch(Func<string, Exception, T> returnValue);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestable<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestable<T> Finally(Action log);
    }

    /// <summary>
    /// 请求能力。
    /// </summary>
    /// <typeparam name="T">结果数据。</typeparam>
    public interface IXmlRequestable<T> : IRequestableExtend<T>
    {
        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        IXmlCatchRequestable<T> XmlCatch(Action<string, XmlException> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IXmlResultCatchRequestable<T> XmlCatch(Func<string, XmlException, T> returnValue);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultCatchRequestable<T> WebCatch(Func<WebException, T> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestable<T> Finally(Action log);
    }

    /// <summary>
    /// 转换能力。
    /// </summary>
    public interface ICastRequestable
    {
        /// <summary>
        /// 数据返回JSON格式的结果，将转为指定类型。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="namingType">命名规则。</param>
        /// <returns></returns>
        IJsonRequestable<T> JsonCast<T>(NamingType namingType = NamingType.Normal) where T : class;
        /// <summary>
        /// 数据返回XML格式的结果，将转为指定类型。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <returns></returns>
        IXmlRequestable<T> XmlCast<T>() where T : class;

        /// <summary>
        /// 数据返回JSON格式的结果，将转为指定类型(匿名对象)。
        /// </summary>
        /// <typeparam name="T">匿名类型。</typeparam>
        /// <param name="anonymousTypeObject">匿名对象。</param>
        /// <param name="namingType">命名规范。</param>
        /// <returns></returns>
        IJsonRequestable<T> JsonCast<T>(T anonymousTypeObject, NamingType namingType = NamingType.Normal) where T : class;
        /// <summary>
        /// 数据返回XML格式的结果，将转为指定类型(匿名对象)。
        /// </summary>
        /// <typeparam name="T">匿名类型。</typeparam>
        /// <param name="anonymousTypeObject">匿名对象。</param>
        /// <returns></returns>
        IXmlRequestable<T> XmlCast<T>(T anonymousTypeObject) where T : class;
    }

    /// <summary>
    /// 结束。
    /// </summary>
    public interface IFinallyStringRequestable : IRequestableExtend<string>
    {
    }

    /// <summary>
    /// 字符串结果。
    /// </summary>
    public interface IResultStringCatchRequestable : IRequestableExtend<string>
    {
        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyStringRequestable Finally(Action log);
    }

    /// <summary>
    /// 结束。
    /// </summary>
    public interface IFinallyRequestable : IRequestableExtend<string>, ICastRequestable, IFileRequestable
    {

    }

    /// <summary>
    /// 结束。
    /// </summary>
    public interface IFinallyRequestable<T> : IRequestableExtend<T>
    {

    }

    /// <summary>
    /// 异常处理的请求。
    /// </summary>
    public interface ICatchRequestable : IRequestableExtend<string>, ICastRequestable, IFileRequestable
    {
        /// <summary>
        /// 捕获Web异常。
        /// </summary>
        /// <param name="log">记录异常信息。</param>
        /// <returns></returns>
        ICatchRequestable WebCatch(Action<WebException> log);

        /// <summary>
        /// 捕获Web异常，并返回结果（返回最后一次的结果）。
        /// </summary>
        /// <param name="returnValue">异常捕获,并返回异常情况下的结果。</param>
        /// <returns></returns>
        IResultStringCatchRequestable WebCatch(Func<WebException, string> returnValue);

        /// <summary>
        /// 始终执行的动作。
        /// </summary>
        /// <param name="log">请求始终会执行的方法。</param>
        /// <returns></returns>
        IFinallyRequestable Finally(Action log);
    }

    /// <summary>
    /// 间隔时间重试。
    /// </summary>
    public interface IRetryIntervalThenRequestable : ICatchRequestable, IFileRequestable
    {
    }

    /// <summary>
    /// 最大重试次数设置。
    /// </summary>
    public interface IRetryThenRequestable : ICatchRequestable, IFileRequestable
    {
#if NET40
        /// <summary>
        /// 失败后，间隔<paramref name="millisecondsTimeout"/>毫秒后重试请求<see cref="System.Threading.Thread.Sleep(int)"/>。
        /// </summary>
        /// <param name="millisecondsTimeout">失败后，间隔多久重试。单位：毫秒。</param>
        /// <returns></returns>
#else
        /// <summary>
        /// 失败后，间隔<paramref name="millisecondsTimeout"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="millisecondsTimeout">失败后，间隔多久重试。单位：毫秒。</param>
        /// <returns></returns>
#endif
        IRetryIntervalThenRequestable RetryInterval(int millisecondsTimeout);

#if NET40
        /// <summary>
        /// 失败后，间隔<paramref name="interval"/>毫秒后重试请求<see cref="System.Threading.Thread.Sleep(int)"/>。
        /// </summary>
        /// <param name="interval">第一个参数：异常，第二个参数：第N次重试，返回间隔多少时间重试请求。单位：毫秒。</param>
        /// <returns></returns>
#else
        /// <summary>
        /// 失败后，间隔<paramref name="interval"/>毫秒后重试请求。【同步请求：<see cref="System.Threading.Thread.Sleep(int)"/>】或【异步请求：<see cref="Task.Delay(int)"/>】。
        /// </summary>
        /// <param name="interval">第一个参数：异常，第二个参数：第N次重试，返回间隔多少时间重试请求。单位：毫秒。</param>
        /// <returns></returns>
#endif
        IRetryIntervalThenRequestable RetryInterval(Func<WebException, int, int> interval);
    }


    /// <summary>
    /// 延续能力的请求。
    /// </summary>
    public interface IThenRequestable : ICatchRequestable, IFileRequestable
    {
        /// <summary>
        /// 如果请求异常，会调用【<paramref name="predicate"/>】，判断是否重试请求。
        /// 多个条件之间是或的关系。
        /// </summary>
        /// <param name="predicate">判断是否重试请求。</param>
        /// <returns></returns>
        IThenRequestable Or(Predicate<WebException> predicate);

        /// <summary>
        /// 设置重试次数。
        /// </summary>
        /// <param name="retryCount">最大重试次数。</param>
        /// <returns></returns>
        IRetryThenRequestable RetryCount(int retryCount);
    }

    /// <summary>
    /// 带有条件的延续能力请求。
    /// </summary>
    public interface IThenAndConditionRequestable : ICatchRequestable, IFileRequestable
    {
        /// <summary>
        /// 所有条件都满足时，重试一次请求。
        /// 多个条件之间是且的关系。
        /// </summary>
        /// <param name="predicate">条件。</param>
        /// <returns></returns>
        IThenAndConditionRequestable And(Predicate<WebException> predicate);

        /// <summary>
        /// 如果请求异常，会调用【<paramref name="match"/>】，判断是否重试请求。
        /// 多个条件是或的关系。
        /// </summary>
        /// <param name="match">判断是否重试请求。</param>
        /// <returns></returns>
        IThenRequestable TryIf(Predicate<WebException> match);

        /// <summary>
        /// 新开一个重试机制，如果请求异常，会调用【<paramref name="then"/>】，并重试一次请求。
        /// </summary>
        /// <param name="then">异常处理事件。</param>
        /// <returns></returns>
        IThenConditionRequestable Then(Action<IRequestableBase, WebException> then);

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER
        /// <summary>
        /// 新开一个重试机制，如果请求异常，会调用【<paramref name="thenAsync"/>】，并重试一次请求。
        /// </summary>
        /// <param name="thenAsync">异常处理事件。</param>
        /// <returns></returns>
        IThenConditionRequestableAsync ThenAsync(Func<IRequestableBase, WebException, Task> thenAsync);
#endif
    }

    /// <summary>
    /// 带有条件的延续能力请求。
    /// </summary>
    public interface IThenConditionRequestable
    {
        /// <summary>
        /// 所有条件都满足时，重试一次请求。
        /// 多个条件之间是且的关系。
        /// </summary>
        /// <param name="predicate">条件。</param>
        /// <returns></returns>
        IThenAndConditionRequestable If(Predicate<WebException> predicate);
    }
}
