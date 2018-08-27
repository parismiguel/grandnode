﻿using Grand.Core.Domain.AdminSearch;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.News;
using Grand.Services.Orders;
using Grand.Services.Topics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class AdminSearchController : BaseAdminController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly INewsService _newsService;
        private readonly IBlogService _blogService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly AdminSearchSettings _adminSearchSettings;
        private readonly ILocalizationService _localizationService;

        public AdminSearchController(IProductService productService, ICategoryService categoryService, IManufacturerService manufacturerService,
            ITopicService topicService, INewsService newsService, IBlogService blogService, ICustomerService customerService, IOrderService orderService,
            AdminSearchSettings adminSearchSettings, ILocalizationService localizationService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._newsService = newsService;
            this._blogService = blogService;
            this._customerService = customerService;
            this._orderService = orderService;
            this._adminSearchSettings = adminSearchSettings;
            this._localizationService = localizationService;
        }

        [HttpPost]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json("error");

            //object = actual result, int = display order for sorting
            List<Tuple<object, int>> result = new List<Tuple<object, int>>();

            if (searchTerm.Length >= _adminSearchSettings.MinSearchTermLength)
            {
                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInProducts)
                {
                    var products = _productService.SearchProducts(keywords: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count());
                    foreach (var product in products)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = product.Name,
                            link = Url.Content("~/Admin/Product/Edit/") + product.Id,
                            source = _localizationService.GetResource("Admin.Catalog.Products")
                        }, _adminSearchSettings.ProductsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInCategories)
                {
                    var categories = _categoryService.GetAllCategories(searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count());
                    foreach (var category in categories)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = category.Name,
                            link = Url.Content("~/Admin/Category/Edit/") + category.Id,
                            source = _localizationService.GetResource("Admin.Catalog.Categories")
                        }, _adminSearchSettings.CategoriesDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInManufacturers)
                {
                    var manufacturers = _manufacturerService.GetAllManufacturers(searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count());
                    foreach (var manufacturer in manufacturers)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = manufacturer.Name,
                            link = Url.Content("~/Admin/Manufacturer/Edit/") + manufacturer.Id,
                            source = _localizationService.GetResource("Admin.Catalog.Manufacturers")
                        }, _adminSearchSettings.ManufacturersDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInTopics)
                {
                    var topics = _topicService.GetAllTopics("", topicSystemName: searchTerm);
                    foreach (var topic in topics)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = topic.SystemName,
                            link = Url.Content("~/Admin/Topic/Edit/") + topic.Id,
                            source = _localizationService.GetResource("Admin.ContentManagement.Topics")
                        }, _adminSearchSettings.TopicsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInNews)
                {
                    var news = _newsService.GetAllNews(newsTitle: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count());
                    foreach (var signleNews in news)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = signleNews.Title,
                            link = Url.Content("~/Admin/News/Edit/") + signleNews.Id,
                            source = _localizationService.GetResource("Admin.ContentManagement.News")
                        }, _adminSearchSettings.NewsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInBlogs)
                {
                    var blogPosts = _blogService.GetAllBlogPosts(blogPostName: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count());
                    foreach (var blogPost in blogPosts)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = blogPost.Title,
                            link = Url.Content("~/Admin/Blog/Edit/") + blogPost.Id,
                            source = _localizationService.GetResource("Admin.ContentManagement.Blog")
                        }, _adminSearchSettings.BlogsDisplayOrder));
                    }
                }

                if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInCustomers)
                {
                    var customersByEmail = _customerService.GetAllCustomers(email: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count());
                    var customersByUsername = _customerService.GetAllCustomers(username: searchTerm, pageSize: _adminSearchSettings.MaxSearchResultsCount - result.Count()
                        - customersByEmail.Count());
                    var combined = customersByEmail.Intersect(customersByUsername);

                    foreach (var customer in combined)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = customer.Email,
                            link = Url.Content("~/Admin/Customer/Edit/") + customer.Id,
                            source = _localizationService.GetResource("Admin.Customers")
                        }, _adminSearchSettings.CustomersDisplayOrder));
                    }
                }
            }

            if (result.Count() < _adminSearchSettings.MaxSearchResultsCount && _adminSearchSettings.SearchInOrders)
            {
                int.TryParse(searchTerm, out int orderNumber);
                if (orderNumber > 0)
                {
                    var order = _orderService.GetOrderByNumber(orderNumber);
                    if (order != null)
                    {
                        result.Add(new Tuple<object, int>(new
                        {
                            title = order.OrderNumber,
                            link = Url.Content("~/Admin/Order/Edit/") + order.Id,
                            source = _localizationService.GetResource("Admin.Orders")
                        }, _adminSearchSettings.OrdersDisplayOrder));
                    }
                }
            }

            result = result.OrderBy(x => x.Item2).ToList();
            return Json(result.Select(x => x.Item1).ToList());
        }
    }
}
