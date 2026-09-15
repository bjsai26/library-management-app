using LibraryManagement.AppServices.DTOs.Category;
using LibraryManagement.AppServices.DTOs.Common;
using LibraryManagement.AppServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

/// <summary>
/// Shelving categories that books belong to. Any signed-in user can read them;
/// only an Admin can change them.
/// </summary>
[Route("api/categories")]
[Authorize]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Returns one page of categories, ordered by name.</summary>
    /// <param name="query">Paging and search options. Search matches the category name.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">A page of categories, each with the number of books in it.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] QueryParameters query,
        CancellationToken cancellationToken)
        => HandleResult(await _categoryService.GetAllAsync(query, cancellationToken));

    /// <summary>Returns a single category and the number of books in it.</summary>
    /// <param name="id">Identifier of the category.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The requested category.</response>
    /// <response code="404">No category exists with that id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => HandleResult(await _categoryService.GetByIdAsync(id, cancellationToken));

    /// <summary>Creates a category. Admin only.</summary>
    /// <param name="request">Name and optional description.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The created category, including its new id.</response>
    /// <response code="400">A field failed validation.</response>
    /// <response code="403">The signed-in user is not an Admin.</response>
    /// <response code="409">A category with that name already exists.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
        => HandleResult(await _categoryService.CreateAsync(request, cancellationToken));

    /// <summary>Replaces the name and description of an existing category. Admin only.</summary>
    /// <param name="id">Identifier of the category to update.</param>
    /// <param name="request">The new values.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The updated category.</response>
    /// <response code="400">A field failed validation.</response>
    /// <response code="403">The signed-in user is not an Admin.</response>
    /// <response code="404">No category exists with that id.</response>
    /// <response code="409">Another category already uses that name.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
        => HandleResult(await _categoryService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Permanently removes a category. Admin only. Blocked while any book still
    /// belongs to it, so deleting a category never removes books as a side effect.
    /// </summary>
    /// <param name="id">Identifier of the category to delete.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The category was deleted.</response>
    /// <response code="403">The signed-in user is not an Admin.</response>
    /// <response code="404">No category exists with that id.</response>
    /// <response code="409">Books still belong to this category.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => HandleResult(await _categoryService.DeleteAsync(id, cancellationToken));
}
