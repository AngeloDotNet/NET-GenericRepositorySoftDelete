using Microsoft.AspNetCore.Mvc;
using Repository.Api.Entities;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var repo = uow.Repository<Product>();
        var list = await repo.GetAllAsync();

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(Product dto)
    {
        var repo = uow.Repository<Product>();

        await repo.AddAsync(dto);
        await uow.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, Product dto)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        product.Name = dto.Name;
        product.Price = dto.Price;

        repo.Update(product);
        await uow.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        await repo.DeleteAsync(product); // soft delete
        await uow.SaveChangesAsync();

        return NoContent();
    }

    // endpoint per hard delete (uso con cautela)
    [HttpDelete("hard/{id:int}")]
    public async Task<IActionResult> HardDeleteAsync(int id)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        await repo.HardDeleteAsync(product);
        await uow.SaveChangesAsync();

        return NoContent();
    }
}