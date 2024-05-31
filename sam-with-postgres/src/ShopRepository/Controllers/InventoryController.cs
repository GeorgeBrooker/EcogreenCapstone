using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopRepository.Data;
using ShopRepository.Dtos;
using ShopRepository.Helper;
using ShopRepository.Models;

namespace ShopRepository.Controllers;

[Route("api/inventory")]
[Produces("application/json")]
public class InventoryController(IShopRepo repo, StockUploadHelper stockUploader) : ControllerBase
{
//
// STOCK PHOTOS
//
    [HttpPost("UploadPhotos/{stockId:guid}")]
    public async Task<ActionResult<bool>> UploadPhotos(Guid stockId)
    {
        var form = Request.Form;
        var files = Request.Form.Keys;
        var images = new byte[files.Count][];

        var i = 0;
        foreach (var key in files)
        {
            images[i] = Convert.FromBase64String(form[key]!);
            i++;
        }

        var result = await stockUploader.UploadImages(images, stockId);

        if (result)
            return Ok();

        return BadRequest("Image upload failed");
    }
}