﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheShoesShop_BackEnd.Auth;
using TheShoesShop_BackEnd.DTOs;
using TheShoesShop_BackEnd.Services;

namespace TheShoesShop_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartDetailController : ControllerBase
    {
        private readonly TheShoesShopServices _TheShoesShopServices;

        public CartDetailController( TheShoesShopServices TheShoesShopServices) 
        { 
            _TheShoesShopServices = TheShoesShopServices;
        }

        // Get cart detail list of customer
        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> GetCartDetailListOfCustomer()
        {
            try
            {

                // Get user
                var User = new User(HttpContext.User);

                // Get cart detail list of customer by customer id
                var CartDetailList = await _TheShoesShopServices._CartDetailService.GetCartDetailListOfCustomer(User.CustomerID);

                return Ok(new Response
                {
                    Success = true,
                    Message = "Get shoes in cart successfully",
                    Data = new { CartDetailList }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new Response
                {
                    Success = true,
                    Message = "Error, try again"
                });
            }
        }

        // Add shoes into cart
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddShoes(CartDetailDTO CartDetail)
        {
            try
            {
                // Check shoes in database
                var Shoes = await _TheShoesShopServices._ShoesService.GetShoesByID(CartDetail.ShoesID);
                if(Shoes == null)
                {
                    return BadRequest(new Response
                    {
                        Success = false,
                        Message = "Shoes is not exist"
                    });
                }

                // Add detail to cart
                var User = new User(HttpContext.User);
                CartDetail.CustomerID = User.CustomerID;
                var NewCartDetail = await _TheShoesShopServices._CartDetailService.AddShoes(CartDetail);

                return Created(
                    HttpContext.Request.Host.Value,
                    new Response
                    {
                        Success = true,
                        Message = "Add shoes to cart successfully",
                        Data = new { NewCartDetail }
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new Response
                {
                    Success = true,
                    Message = "Error, try again"
                });
            }
        }

        // Delete shoes from cart
        [HttpDelete("remove/{ShoesID}")]
        [Authorize]
        public async Task<IActionResult> RemoveShoes(int ShoesID)
        {
            try
            {
                // Check shoes in database
                var Shoes = await _TheShoesShopServices._ShoesService.GetShoesByID(ShoesID);
                if (Shoes == null)
                {
                    return BadRequest(new Response
                    {
                        Success = false,
                        Message = "Shoes is not exist"
                    });
                }

                // Remove detail to cart
                var User = new User(HttpContext.User);
                var CartDetail = new CartDetailDTO { CustomerID = User.CustomerID, ShoesID = ShoesID};
                var RemoveStatus = await _TheShoesShopServices._CartDetailService.RemoveShoes(CartDetail);
                if (!RemoveStatus)
                {
                    return BadRequest(new Response
                    {
                        Success = true,
                        Message = "ShoesID of User is not exist"
                    });
                }

                return Ok( new Response
                    {
                        Success = true,
                        Message = "Remove shoes to cart successfully"
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new Response
                {
                    Success = true,
                    Message = "Error, try again"
                });
            }
        }

        // Update shoes quantity to cart
        [HttpPatch("update/quantity")]
        [Authorize]
        public async Task<IActionResult> UpdateShoesAmount(CartDetailDTO CartDetail)
        {
            try
            {
                // Check shoes in database
                var Shoes = await _TheShoesShopServices._ShoesService.GetShoesByID(CartDetail.ShoesID);
                if (Shoes == null)
                {
                    return BadRequest(new Response
                    {
                        Success = false,
                        Message = "Shoes is not exist"
                    });
                }

                // Update shoes quanity to cart
                var User = new User(HttpContext.User);
                CartDetail.CustomerID = User.CustomerID;
                var NewCartDetail = await _TheShoesShopServices._CartDetailService.UpdateShoesAmount(CartDetail);
                if (NewCartDetail == null)
                {
                    return BadRequest(new Response
                    {
                        Success = true,
                        Message = "ShoesID of User is not exist"
                    });
                }

                return Ok(new Response
                {
                    Success = true,
                    Message = "Update shoes amount successfully",
                    Data = new { NewCartDetail }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new Response
                {
                    Success = true,
                    Message = "Error, try again"
                });
            }
        }
    }
}
