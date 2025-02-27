﻿using Boilerplate.Shared.Dtos.Products;
using Boilerplate.Shared.Controllers.Products;

namespace Boilerplate.Client.Core.Components.Pages;

public partial class ProductPage
{
    protected override string? Title => string.Empty;
    protected override string? Subtitle => string.Empty;


    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Name { get; set; }


    [AutoInject] private IProductViewController productViewController = default!;


    [CascadingParameter] private BitDir? currentDir { get; set; }


    private ProductDto? product;
    private List<ProductDto>? similarProducts;
    private List<ProductDto>? siblingProducts;
    private bool isLoadingProduct = true;
    private bool isLoadingSimilarProducts = true;
    private bool isLoadingSiblingProducts = true;


    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        await LoadProduct();

        if (InPrerenderSession is false)
        {
            await Task.WhenAll(LoadSimilarProducts(), LoadSiblingProducts());
        }
    }

    private async Task LoadProduct()
    {
        try
        {
            product = await productViewController.Get(Id, CurrentCancellationToken);
        }
        finally
        {
            isLoadingProduct = false;
            StateHasChanged();
        }
    }

    private async Task LoadSimilarProducts()
    {
        if (product is null) return;

        try
        {
            similarProducts = await productViewController
                .WithQuery(new ODataQuery { Top = 10 })
                .GetSimilar(product.Id, CurrentCancellationToken);
        }
        finally
        {
            isLoadingSimilarProducts = false;
            StateHasChanged();
        }
    }

    private async Task LoadSiblingProducts()
    {
        if (product is null || product.CategoryId.HasValue is false) return;

        try
        {
            siblingProducts = await productViewController
                .WithQuery(new ODataQuery { Top = 10, Filter = $"{nameof(ProductDto.Id)} ne {product.Id}" })
                .GetSiblings(product.CategoryId.Value, CurrentCancellationToken);
        }
        finally
        {
            isLoadingSiblingProducts = false;
            StateHasChanged();
        }
    }


    private string? GetProductImageUrl(ProductDto product) => product.GetProductImageUrl(AbsoluteServerAddress);
}
