﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    public abstract class PageModel
    {
        private PageArgumentBinder _binder;
        private IUrlHelper _urlHelper;

        /// <summary>
        /// Gets or sets the <see cref="PageArgumentBinder"/>.
        /// </summary>
        public PageArgumentBinder Binder
        {
            get
            {
                if (_binder == null)
                {
                    _binder = HttpContext?.RequestServices?.GetRequiredService<PageArgumentBinder>();
                }

                return _binder;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _binder = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlHelper"/>.
        /// </summary>
        public IUrlHelper Url
        {
            get
            {
                if (_urlHelper == null)
                {
                    var factory = HttpContext?.RequestServices?.GetRequiredService<IUrlHelperFactory>();
                    _urlHelper = factory?.GetUrlHelper(PageContext);
                }

                return _urlHelper;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _urlHelper = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="RazorPages.Page"/> instance this model belongs to.
        /// </summary>
        public Page Page => PageContext?.Page;

        /// <summary>
        /// Gets the <see cref="RazorPages.PageContext"/>.
        /// </summary>
        [PageContext]
        public PageContext PageContext { get; set; }

        /// <summary>
        /// Gets the <see cref="ViewContext"/>.
        /// </summary>
        public ViewContext ViewContext => PageContext;

        /// <summary>
        /// Gets the <see cref="Http.HttpContext"/>.
        /// </summary>
        public HttpContext HttpContext => PageContext?.HttpContext;

        /// <summary>
        /// Gets the <see cref="HttpRequest"/>.
        /// </summary>
        public HttpRequest Request => HttpContext?.Request;

        /// <summary>
        /// Gets the <see cref="HttpResponse"/>.
        /// </summary>
        public HttpResponse Response => HttpContext?.Response;

        /// <summary>
        /// Gets the <see cref="ModelStateDictionary"/>.
        /// </summary>
        public ModelStateDictionary ModelState => PageContext.ModelState;

        /// <summary>
        /// Gets the <see cref="ITempDataDictionary"/> from the <see cref="PageContext"/>.
        /// </summary>
        /// <remarks>Returns null if <see cref="PageContext"/> is null.</remarks>
        public ITempDataDictionary TempData => PageContext?.TempData;

        /// <summary>
        /// Gets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public ViewDataDictionary ViewData => PageContext?.ViewData;

        /// <summary>
        /// Binds the model with the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="name">The model name.</param>
        /// <returns>A <see cref="Task"/> that on completion returns the bound model.</returns>
        protected internal Task<TModel> BindAsync<TModel>(string name)
        {
            return Binder.BindModelAsync<TModel>(PageContext, name);
        }

        /// <summary>
        /// Binds the model with the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="name">The model name.</param>
        /// <param name="default">The default model value.</param>
        /// <returns>A <see cref="Task"/> that on completion returns the bound model.</returns>
        protected internal Task<TModel> BindAsync<TModel>(TModel @default, string name)
        {
            return Binder.BindModelAsync(PageContext, @default, name);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using values from the pageModel's current
        /// <see cref="IValueProvider"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        protected internal Task<bool> TryUpdateModelAsync<TModel>(TModel model)
        {
            return Binder.TryUpdateModelAsync(PageContext, model);
        }

        /// <summary>
        /// Updates the specified <paramref name="model"/> instance using values from the pageModel's current
        /// <see cref="IValueProvider"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model object.</typeparam>
        /// <param name="model">The model instance to update.</param>
        /// <param name="name">The model name.</param>
        /// <returns>A <see cref="Task"/> that on completion returns <c>true</c> if the update is successful.</returns>
        protected internal Task<bool> TryUpdateModelAsync<TModel>(TModel model, string name)
        {
            return Binder.TryUpdateModelAsync(PageContext, model, name);
        }

        #region Factory methods
        /// <summary>
        /// Creates a <see cref="ChallengeResult"/>.
        /// </summary>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        /// <remarks>
        /// The behavior of this method depends on the <see cref="AuthenticationManager"/> in use.
        /// <see cref="StatusCodes.Status401Unauthorized"/> and <see cref="StatusCodes.Status403Forbidden"/>
        /// are among likely status results.
        /// </remarks>
        public virtual ChallengeResult Challenge()
            => new ChallengeResult();

        /// <summary>
        /// Creates a <see cref="ChallengeResult"/> with the specified authentication schemes.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        /// <remarks>
        /// The behavior of this method depends on the <see cref="AuthenticationManager"/> in use.
        /// <see cref="StatusCodes.Status401Unauthorized"/> and <see cref="StatusCodes.Status403Forbidden"/>
        /// are among likely status results.
        /// </remarks>
        public virtual ChallengeResult Challenge(params string[] authenticationSchemes)
            => new ChallengeResult(authenticationSchemes);

        /// <summary>
        /// Creates a <see cref="ChallengeResult"/> with the specified <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        /// <remarks>
        /// The behavior of this method depends on the <see cref="AuthenticationManager"/> in use.
        /// <see cref="StatusCodes.Status401Unauthorized"/> and <see cref="StatusCodes.Status403Forbidden"/>
        /// are among likely status results.
        /// </remarks>
        public virtual ChallengeResult Challenge(AuthenticationProperties properties)
            => new ChallengeResult(properties);

        /// <summary>
        /// Creates a <see cref="ChallengeResult"/> with the specified specified authentication schemes and
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ChallengeResult"/> for the response.</returns>
        /// <remarks>
        /// The behavior of this method depends on the <see cref="AuthenticationManager"/> in use.
        /// <see cref="StatusCodes.Status401Unauthorized"/> and <see cref="StatusCodes.Status403Forbidden"/>
        /// are among likely status results.
        /// </remarks>
        public virtual ChallengeResult Challenge(
            AuthenticationProperties properties,
            params string[] authenticationSchemes)
            => new ChallengeResult(authenticationSchemes, properties);

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object with <see cref="StatusCodes.Status200OK"/> by specifying a
        /// <paramref name="content"/> string.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        public virtual ContentResult Content(string content)
            => Content(content, (MediaTypeHeaderValue)null);

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object with <see cref="StatusCodes.Status200OK"/> by specifying a 
        /// <paramref name="content"/> string and a content type.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        public virtual ContentResult Content(string content, string contentType)
            => Content(content, MediaTypeHeaderValue.Parse(contentType));

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object with <see cref="StatusCodes.Status200OK"/> by specifying a 
        /// <paramref name="content"/> string, a <paramref name="contentType"/>, and <paramref name="contentEncoding"/>.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <param name="contentEncoding">The content encoding.</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        /// <remarks>
        /// If encoding is provided by both the 'charset' and the <paramref name="contentEncoding"/> parameters, then
        /// the <paramref name="contentEncoding"/> parameter is chosen as the final encoding.
        /// </remarks>
        public virtual ContentResult Content(string content, string contentType, Encoding contentEncoding)
        {
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
            mediaTypeHeaderValue.Encoding = contentEncoding ?? mediaTypeHeaderValue.Encoding;
            return Content(content, mediaTypeHeaderValue);
        }

        /// <summary>
        /// Creates a <see cref="ContentResult"/> object with <see cref="StatusCodes.Status200OK"/> by specifying a 
        /// <paramref name="content"/> string and a <paramref name="contentType"/>.
        /// </summary>
        /// <param name="content">The content to write to the response.</param>
        /// <param name="contentType">The content type (MIME type).</param>
        /// <returns>The created <see cref="ContentResult"/> object for the response.</returns>
        public virtual ContentResult Content(string content, MediaTypeHeaderValue contentType)
        {
            return new ContentResult
            {
                Content = content,
                ContentType = contentType?.ToString()
            };
        }

        /// <summary>
        /// Creates a <see cref="ForbidResult"/> (<see cref="StatusCodes.Status403Forbidden"/> by default).
        /// </summary>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        /// <remarks>
        /// Some authentication schemes, such as cookies, will convert <see cref="StatusCodes.Status403Forbidden"/> to
        /// a redirect to show a login page.
        /// </remarks>
        public virtual ForbidResult Forbid()
            => new ForbidResult();

        /// <summary>
        /// Creates a <see cref="ForbidResult"/> (<see cref="StatusCodes.Status403Forbidden"/> by default) with the
        /// specified authentication schemes.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        /// <remarks>
        /// Some authentication schemes, such as cookies, will convert <see cref="StatusCodes.Status403Forbidden"/> to
        /// a redirect to show a login page.
        /// </remarks>
        public virtual ForbidResult Forbid(params string[] authenticationSchemes)
            => new ForbidResult(authenticationSchemes);

        /// <summary>
        /// Creates a <see cref="ForbidResult"/> (<see cref="StatusCodes.Status403Forbidden"/> by default) with the
        /// specified <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        /// <remarks>
        /// Some authentication schemes, such as cookies, will convert <see cref="StatusCodes.Status403Forbidden"/> to 
        /// a redirect to show a login page.
        /// </remarks>
        public virtual ForbidResult Forbid(AuthenticationProperties properties)
            => new ForbidResult(properties);

        /// <summary>
        /// Creates a <see cref="ForbidResult"/> (<see cref="StatusCodes.Status403Forbidden"/> by default) with the 
        /// specified specified authentication schemes and <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <returns>The created <see cref="ForbidResult"/> for the response.</returns>
        /// <remarks>
        /// Some authentication schemes, such as cookies, will convert <see cref="StatusCodes.Status403Forbidden"/> to
        /// a redirect to show a login page.
        /// </remarks>
        public virtual ForbidResult Forbid(AuthenticationProperties properties, params string[] authenticationSchemes)
            => new ForbidResult(authenticationSchemes, properties);

        /// <summary>
        /// Returns a file with the specified <paramref name="fileContents" /> as content
        /// (<see cref="StatusCodes.Status200OK"/>) and the specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="FileContentResult"/> for the response.</returns>
        public virtual FileContentResult File(byte[] fileContents, string contentType)
            => File(fileContents, contentType, fileDownloadName: null);

        /// <summary>
        /// Returns a file with the specified <paramref name="fileContents" /> as content (<see cref="StatusCodes.Status200OK"/>), the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="FileContentResult"/> for the response.</returns>
        public virtual FileContentResult File(byte[] fileContents, string contentType, string fileDownloadName)
            => new FileContentResult(fileContents, contentType) { FileDownloadName = fileDownloadName };

        /// <summary>
        /// Returns a file in the specified <paramref name="fileStream" /> (<see cref="StatusCodes.Status200OK"/>)
        /// with the specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="fileStream">The <see cref="Stream"/> with the contents of the file.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="FileStreamResult"/> for the response.</returns>
        public virtual FileStreamResult File(Stream fileStream, string contentType)
            => File(fileStream, contentType, fileDownloadName: null);

        /// <summary>
        /// Returns a file in the specified <paramref name="fileStream" /> (<see cref="StatusCodes.Status200OK"/>) with the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="fileStream">The <see cref="Stream"/> with the contents of the file.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="FileStreamResult"/> for the response.</returns>
        public virtual FileStreamResult File(Stream fileStream, string contentType, string fileDownloadName)
            => new FileStreamResult(fileStream, contentType) { FileDownloadName = fileDownloadName };

        /// <summary>
        /// Returns the file specified by <paramref name="virtualPath" /> (<see cref="StatusCodes.Status200OK"/>) with the
        /// specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="VirtualFileResult"/> for the response.</returns>
        public virtual VirtualFileResult File(string virtualPath, string contentType)
            => File(virtualPath, contentType, fileDownloadName: null);

        /// <summary>
        /// Returns the file specified by <paramref name="virtualPath" /> (<see cref="StatusCodes.Status200OK"/>) with the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="VirtualFileResult"/> for the response.</returns>
        public virtual VirtualFileResult File(string virtualPath, string contentType, string fileDownloadName)
            => new VirtualFileResult(virtualPath, contentType) { FileDownloadName = fileDownloadName };

        /// <summary>
        /// Creates a <see cref="LocalRedirectResult"/> object that redirects 
        /// (<see cref="StatusCodes.Status302Found"/>) to the specified local <paramref name="localUrl"/>.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <returns>The created <see cref="LocalRedirectResult"/> for the response.</returns>
        public virtual LocalRedirectResult LocalRedirect(string localUrl)
        {
            if (string.IsNullOrEmpty(localUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(localUrl));
            }

            return new LocalRedirectResult(localUrl);
        }

        /// <summary>
        /// Creates a <see cref="LocalRedirectResult"/> object with <see cref="LocalRedirectResult.Permanent"/> set to
        /// true (<see cref="StatusCodes.Status301MovedPermanently"/>) using the specified <paramref name="localUrl"/>.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <returns>The created <see cref="LocalRedirectResult"/> for the response.</returns>
        public virtual LocalRedirectResult LocalRedirectPermanent(string localUrl)
        {
            if (string.IsNullOrEmpty(localUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(localUrl));
            }

            return new LocalRedirectResult(localUrl, permanent: true);
        }

        /// <summary>
        /// Creates a <see cref="LocalRedirectResult"/> object with <see cref="LocalRedirectResult.Permanent"/> set to
        /// false and <see cref="LocalRedirectResult.PreserveMethod"/> set to true 
        /// (<see cref="StatusCodes.Status307TemporaryRedirect"/>) using the specified <paramref name="localUrl"/>.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <returns>The created <see cref="LocalRedirectResult"/> for the response.</returns>
        public virtual LocalRedirectResult LocalRedirectPreserveMethod(string localUrl)
        {
            if (string.IsNullOrEmpty(localUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(localUrl));
            }

            return new LocalRedirectResult(localUrl: localUrl, permanent: false, preserveMethod: true);
        }

        /// <summary>
        /// Creates a <see cref="LocalRedirectResult"/> object with <see cref="LocalRedirectResult.Permanent"/> set to
        /// true and <see cref="LocalRedirectResult.PreserveMethod"/> set to true 
        /// (<see cref="StatusCodes.Status308PermanentRedirect"/>) using the specified <paramref name="localUrl"/>.
        /// </summary>
        /// <param name="localUrl">The local URL to redirect to.</param>
        /// <returns>The created <see cref="LocalRedirectResult"/> for the response.</returns>
        public virtual LocalRedirectResult LocalRedirectPermanentPreserveMethod(string localUrl)
        {
            if (string.IsNullOrEmpty(localUrl))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(localUrl));
            }

            return new LocalRedirectResult(localUrl: localUrl, permanent: true, preserveMethod: true);
        }

        /// <summary>
        /// Creates an <see cref="NotFoundResult"/> that produces a <see cref="StatusCodes.Status404NotFound"/> response.
        /// </summary>
        /// <returns>The created <see cref="NotFoundResult"/> for the response.</returns>
        public virtual NotFoundResult NotFound()
            => new NotFoundResult();

        /// <summary>
        /// Creates an <see cref="NotFoundObjectResult"/> that produces a <see cref="StatusCodes.Status404NotFound"/> response.
        /// </summary>
        /// <returns>The created <see cref="NotFoundObjectResult"/> for the response.</returns>
        public virtual NotFoundObjectResult NotFound(object value)
            => new NotFoundObjectResult(value);

        /// <summary>
        /// Returns the file specified by <paramref name="physicalPath" /> (<see cref="StatusCodes.Status200OK"/>) with the
        /// specified <paramref name="contentType" /> as the Content-Type.
        /// </summary>
        /// <param name="physicalPath">The physical path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <returns>The created <see cref="PhysicalFileResult"/> for the response.</returns>
        public virtual PhysicalFileResult PhysicalFile(string physicalPath, string contentType)
            => PhysicalFile(physicalPath, contentType, fileDownloadName: null);

        /// <summary>
        /// Returns the file specified by <paramref name="physicalPath" /> (<see cref="StatusCodes.Status200OK"/>) with the
        /// specified <paramref name="contentType" /> as the Content-Type and the
        /// specified <paramref name="fileDownloadName" /> as the suggested file name.
        /// </summary>
        /// <param name="physicalPath">The physical path of the file to be returned.</param>
        /// <param name="contentType">The Content-Type of the file.</param>
        /// <param name="fileDownloadName">The suggested file name.</param>
        /// <returns>The created <see cref="PhysicalFileResult"/> for the response.</returns>
        public virtual PhysicalFileResult PhysicalFile(
            string physicalPath,
            string contentType,
            string fileDownloadName)
            => new PhysicalFileResult(physicalPath, contentType) { FileDownloadName = fileDownloadName };

        /// <summary>
        /// Creates a <see cref="RedirectResult"/> object that redirects (<see cref="StatusCodes.Status302Found"/>)
        /// to the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <returns>The created <see cref="RedirectResult"/> for the response.</returns>
        protected internal RedirectResult Redirect(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(url));
            }

            return new RedirectResult(url);
        }

        /// <summary>
        /// Creates a <see cref="RedirectResult"/> object with <see cref="RedirectResult.Permanent"/> set to true
        /// (<see cref="StatusCodes.Status301MovedPermanently"/>) using the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <returns>The created <see cref="RedirectResult"/> for the response.</returns>
        public virtual RedirectResult RedirectPermanent(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(url));
            }

            return new RedirectResult(url, permanent: true);
        }

        /// <summary>
        /// Creates a <see cref="RedirectResult"/> object with <see cref="RedirectResult.Permanent"/> set to false
        /// and <see cref="RedirectResult.PreserveMethod"/> set to true (<see cref="StatusCodes.Status307TemporaryRedirect"/>) 
        /// using the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <returns>The created <see cref="RedirectResult"/> for the response.</returns>
        public virtual RedirectResult RedirectPreserveMethod(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(url));
            }

            return new RedirectResult(url: url, permanent: false, preserveMethod: true);
        }

        /// <summary>
        /// Creates a <see cref="RedirectResult"/> object with <see cref="RedirectResult.Permanent"/> set to true
        /// and <see cref="RedirectResult.PreserveMethod"/> set to true (<see cref="StatusCodes.Status308PermanentRedirect"/>) 
        /// using the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <returns>The created <see cref="RedirectResult"/> for the response.</returns>
        public virtual RedirectResult RedirectPermanentPreserveMethod(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(url));
            }

            return new RedirectResult(url: url, permanent: true, preserveMethod: true);
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified action using the <paramref name="actionName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToAction(string actionName)
            => RedirectToAction(actionName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified action using the 
        /// <paramref name="actionName"/> and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToAction(string actionName, object routeValues)
            => RedirectToAction(actionName, controllerName: null, routeValues: routeValues);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified action using the 
        /// <paramref name="actionName"/> and the <paramref name="controllerName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToAction(string actionName, string controllerName)
            => RedirectToAction(actionName, controllerName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified action using the specified
        /// <paramref name="actionName"/>, <paramref name="controllerName"/>, and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToAction(
            string actionName,
            string controllerName,
            object routeValues)
            => RedirectToAction(actionName, controllerName, routeValues, fragment: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified action using the specified
        /// <paramref name="actionName"/>, <paramref name="controllerName"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToAction(
            string actionName,
            string controllerName,
            string fragment)
            => RedirectToAction(actionName, controllerName, routeValues: null, fragment: fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified action using the specified <paramref name="actionName"/>,
        /// <paramref name="controllerName"/>, <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToAction(
            string actionName,
            string controllerName,
            object routeValues,
            string fragment)
        {
            return new RedirectToActionResult(actionName, controllerName, routeValues, fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status307TemporaryRedirect"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to false and <see cref="RedirectToActionResult.PreserveMethod"/> 
        /// set to true, using the specified <paramref name="actionName"/>, <paramref name="controllerName"/>, 
        /// <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>       
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToActionPreserveMethod(
            string actionName = null,
            string controllerName = null,
            object routeValues = null,
            string fragment = null)
        {
            return new RedirectToActionResult(
                actionName: actionName,
                controllerName: controllerName,
                routeValues: routeValues,
                permanent: false,
                preserveMethod: true,
                fragment: fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to true using the specified <paramref name="actionName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName)
            => RedirectToActionPermanent(actionName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to true using the specified <paramref name="actionName"/> 
        /// and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName, object routeValues)
            => RedirectToActionPermanent(actionName, controllerName: null, routeValues: routeValues);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to true using the specified <paramref name="actionName"/> 
        /// and <paramref name="controllerName"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName, string controllerName)
            => RedirectToActionPermanent(actionName, controllerName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to true using the specified <paramref name="actionName"/>,
        /// <paramref name="controllerName"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToActionPermanent(
            string actionName,
            string controllerName,
            string fragment)
            => RedirectToActionPermanent(actionName, controllerName, routeValues: null, fragment: fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to true using the specified <paramref name="actionName"/>,
        /// <paramref name="controllerName"/>, and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToActionPermanent(
            string actionName,
            string controllerName,
            object routeValues)
            => RedirectToActionPermanent(actionName, controllerName, routeValues, fragment: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to true using the specified <paramref name="actionName"/>,
        /// <paramref name="controllerName"/>, <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>
        public virtual RedirectToActionResult RedirectToActionPermanent(
            string actionName,
            string controllerName,
            object routeValues,
            string fragment)
        {
            return new RedirectToActionResult(
                actionName,
                controllerName,
                routeValues,
                permanent: true,
                fragment: fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status308PermanentRedirect"/>) to the specified action with 
        /// <see cref="RedirectToActionResult.Permanent"/> set to true and <see cref="RedirectToActionResult.PreserveMethod"/>
        /// set to true, using the specified <paramref name="actionName"/>, <paramref name="controllerName"/>, 
        /// <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="controllerName">The name of the pageModel.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToActionResult"/> for the response.</returns>        
        public virtual RedirectToActionResult RedirectToActionPermanentPreserveMethod(
            string actionName = null,
            string controllerName = null,
            object routeValues = null,
            string fragment = null)
        {
            return new RedirectToActionResult(
                actionName: actionName,
                controllerName: controllerName,
                routeValues: routeValues,
                permanent: true,
                preserveMethod: true,
                fragment: fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified route using the specified <paramref name="routeName"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoute(string routeName)
            => RedirectToRoute(routeName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified route using the specified <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoute(object routeValues)
            => RedirectToRoute(routeName: null, routeValues: routeValues);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified route using the specified
        /// <paramref name="routeName"/> and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoute(string routeName, object routeValues)
            => RedirectToRoute(routeName, routeValues, fragment: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified route using the specified
        /// <paramref name="routeName"/> and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoute(string routeName, string fragment)
            => RedirectToRoute(routeName, routeValues: null, fragment: fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified route using the specified
        /// <paramref name="routeName"/>, <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoute(
            string routeName,
            object routeValues,
            string fragment)
        {
            return new RedirectToRouteResult(routeName, routeValues, fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status307TemporaryRedirect"/>) to the specified route with 
        /// <see cref="RedirectToRouteResult.Permanent"/> set to false and <see cref="RedirectToRouteResult.PreserveMethod"/>
        /// set to true, using the specified <paramref name="routeName"/>, <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>       
        public virtual RedirectToRouteResult RedirectToRoutePreserveMethod(
            string routeName = null,
            object routeValues = null,
            string fragment = null)
        {
            return new RedirectToRouteResult(
                routeName: routeName,
                routeValues: routeValues,
                permanent: false,
                preserveMethod: true,
                fragment: fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified route with 
        /// <see cref="RedirectToRouteResult.Permanent"/> set to true using the specified <paramref name="routeName"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName)
            => RedirectToRoutePermanent(routeName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified route with 
        /// <see cref="RedirectToRouteResult.Permanent"/> set to true using the specified <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoutePermanent(object routeValues)
            => RedirectToRoutePermanent(routeName: null, routeValues: routeValues);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified route with
        /// <see cref="RedirectToRouteResult.Permanent"/> set to true using the specified <paramref name="routeName"/>
        /// and <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName, object routeValues)
            => RedirectToRoutePermanent(routeName, routeValues, fragment: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified route with 
        /// <see cref="RedirectToRouteResult.Permanent"/> set to true using the specified <paramref name="routeName"/>
        /// and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName, string fragment)
            => RedirectToRoutePermanent(routeName, routeValues: null, fragment: fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified route with
        /// <see cref="RedirectToRouteResult.Permanent"/> set to true using the specified <paramref name="routeName"/>,
        /// <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>
        public virtual RedirectToRouteResult RedirectToRoutePermanent(
            string routeName,
            object routeValues,
            string fragment)
        {
            return new RedirectToRouteResult(routeName, routeValues, permanent: true, fragment: fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status308PermanentRedirect"/>) to the specified route with
        /// <see cref="RedirectToRouteResult.Permanent"/> set to true and <see cref="RedirectToRouteResult.PreserveMethod"/>
        /// set to true, using the specified <paramref name="routeName"/>, <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>       
        public virtual RedirectToRouteResult RedirectToRoutePermanentPreserveMethod(
            string routeName = null,
            object routeValues = null,
            string fragment = null)
        {
            return new RedirectToRouteResult(
                routeName: routeName,
                routeValues: routeValues,
                permanent: true,
                preserveMethod: true,
                fragment: fragment)
            {
                UrlHelper = Url,
            };
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the current page.
        /// </summary>
        /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
        public virtual RedirectToPageResult RedirectToPage()
            => RedirectToPage(pageName: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the current page with the specified <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
        public virtual RedirectToPageResult RedirectToPage(object routeValues)
            => RedirectToPage(pageName: null, routeValues: routeValues);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="pageName"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
        protected internal RedirectToPageResult RedirectToPage(string pageName)
            => RedirectToPage(pageName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="pageName"/>
        /// using the specified <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
        protected internal RedirectToPageResult RedirectToPage(string pageName, object routeValues)
            => RedirectToPage(pageName, routeValues, fragment: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="pageName"/>
        /// using the specified <paramref name="fragment"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
        protected internal RedirectToPageResult RedirectToPage(string pageName, string fragment)
            => RedirectToPage(pageName, routeValues: null, fragment: fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status302Found"/>) to the specified <paramref name="pageName"/>
        /// using the specified <paramref name="routeValues"/> and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The <see cref="RedirectToPageResult"/>.</returns>
        protected internal RedirectToPageResult RedirectToPage(string pageName, object routeValues, string fragment)
            => new RedirectToPageResult(pageName, routeValues, fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified <paramref name="pageName"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <returns>The <see cref="RedirectToPageResult"/> with <see cref="RedirectToPageResult.Permanent"/> set.</returns>
        protected internal RedirectToPageResult RedirectToPagePermanent(string pageName)
            => RedirectToPagePermanent(pageName, routeValues: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified <paramref name="pageName"/>
        /// using the specified <paramref name="routeValues"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <returns>The <see cref="RedirectToPageResult"/> with <see cref="RedirectToPageResult.Permanent"/> set.</returns>
        protected internal RedirectToPageResult RedirectToPagePermanent(string pageName, object routeValues)
            => RedirectToPagePermanent(pageName, routeValues, fragment: null);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified <paramref name="pageName"/>
        /// using the specified <paramref name="fragment"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The <see cref="RedirectToPageResult"/> with <see cref="RedirectToPageResult.Permanent"/> set.</returns>
        protected internal RedirectToPageResult RedirectToPagePermanent(string pageName, string fragment)
            => RedirectToPagePermanent(pageName, routeValues: null, fragment: fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status301MovedPermanently"/>) to the specified <paramref name="pageName"/>
        /// using the specified <paramref name="routeValues"/> and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="routeValues">The parameters for a route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The <see cref="RedirectToPageResult"/> with <see cref="RedirectToPageResult.Permanent"/> set.</returns>
        protected internal RedirectToPageResult RedirectToPagePermanent(string pageName, object routeValues, string fragment)
            => new RedirectToPageResult(pageName, routeValues, permanent: true, fragment: fragment);

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status307TemporaryRedirect"/>) to the specified page with 
        /// <see cref="RedirectToRouteResult.Permanent"/> set to false and <see cref="RedirectToRouteResult.PreserveMethod"/>
        /// set to true, using the specified <paramref name="pageName"/>, <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns> 
        public virtual RedirectToPageResult RedirectToPagePreserveMethod(
            string pageName = null,
            object routeValues = null,
            string fragment = null)
        {
            return new RedirectToPageResult(
                pageName: pageName,
                routeValues: routeValues,
                permanent: false,
                preserveMethod: true,
                fragment: fragment);
        }

        /// <summary>
        /// Redirects (<see cref="StatusCodes.Status308PermanentRedirect"/>) to the specified route with
        /// <see cref="RedirectToRouteResult.Permanent"/> set to true and <see cref="RedirectToRouteResult.PreserveMethod"/>
        /// set to true, using the specified <paramref name="pageName"/>, <paramref name="routeValues"/>, and <paramref name="fragment"/>.
        /// </summary>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        /// <returns>The created <see cref="RedirectToRouteResult"/> for the response.</returns>  
        public virtual RedirectToPageResult RedirectToPagePermanentPreserveMethod(
            string pageName = null,
            object routeValues = null,
            string fragment = null)
        {
            return new RedirectToPageResult(
                pageName: pageName,
                routeValues: routeValues,
                permanent: true,
                preserveMethod: true,
                fragment: fragment);
        }

        /// <summary>
        /// Creates a <see cref="SignInResult"/> with the specified authentication scheme.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> containing the user claims.</param>
        /// <param name="authenticationScheme">The authentication scheme to use for the sign-in operation.</param>
        /// <returns>The created <see cref="SignInResult"/> for the response.</returns>
        public virtual SignInResult SignIn(ClaimsPrincipal principal, string authenticationScheme)
            => new SignInResult(authenticationScheme, principal);

        /// <summary>
        /// Creates a <see cref="SignInResult"/> with the specified specified authentication scheme and
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> containing the user claims.</param>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-in operation.</param>
        /// <param name="authenticationScheme">The authentication scheme to use for the sign-in operation.</param>
        /// <returns>The created <see cref="SignInResult"/> for the response.</returns>
        public virtual SignInResult SignIn(
            ClaimsPrincipal principal,
            AuthenticationProperties properties,
            string authenticationScheme)
            => new SignInResult(authenticationScheme, principal, properties);

        /// <summary>
        /// Creates a <see cref="SignOutResult"/> with the specified authentication schemes.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication schemes to use for the sign-out operation.</param>
        /// <returns>The created <see cref="SignOutResult"/> for the response.</returns>
        public virtual SignOutResult SignOut(params string[] authenticationSchemes)
            => new SignOutResult(authenticationSchemes);

        /// <summary>
        /// Creates a <see cref="SignOutResult"/> with the specified specified authentication schemes and
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
        /// <param name="authenticationSchemes">The authentication scheme to use for the sign-out operation.</param>
        /// <returns>The created <see cref="SignOutResult"/> for the response.</returns>
        public virtual SignOutResult SignOut(AuthenticationProperties properties, params string[] authenticationSchemes)
            => new SignOutResult(authenticationSchemes, properties);

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> object by specifying a <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="statusCode">The status code to set on the response.</param>
        /// <returns>The created <see cref="StatusCodeResult"/> object for the response.</returns>
        public virtual StatusCodeResult StatusCode(int statusCode)
            => new StatusCodeResult(statusCode);

        /// <summary>
        /// Creates a <see cref="ObjectResult"/> object by specifying a <paramref name="statusCode"/> and <paramref name="value"/>
        /// </summary>
        /// <param name="statusCode">The status code to set on the response.</param>
        /// <param name="value">The value to set on the <see cref="ObjectResult"/>.</param>
        /// <returns>The created <see cref="ObjectResult"/> object for the response.</returns>
        public virtual ObjectResult StatusCode(int statusCode, object value)
        {
            return new ObjectResult(value)
            {
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Creates an <see cref="UnauthorizedResult"/> that produces an <see cref="StatusCodes.Status401Unauthorized"/> response.
        /// </summary>
        /// <returns>The created <see cref="UnauthorizedResult"/> for the response.</returns>
        public virtual UnauthorizedResult Unauthorized()
            => new UnauthorizedResult();

        /// <summary>
        /// Creates a <see cref="PageViewResult"/> object that renders the page.
        /// </summary>
        /// <returns>The <see cref="PageViewResult"/>.</returns>
        protected internal PageViewResult View()
        {
            return new PageViewResult(Page);
        }
        #endregion Factory methods
    }
}