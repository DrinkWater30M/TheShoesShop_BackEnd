﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TheShoesShop_BackEnd.DTOs;
using TheShoesShop_BackEnd.Models;

namespace TheShoesShop_BackEnd.Services
{
    public class CartDetailService
    {
        private readonly TheShoesShopDbContext _context;
        private readonly IMapper _mapper;

        public CartDetailService(TheShoesShopDbContext context, IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CartDetailDTO> AddShoes(CartDetailDTO CartDetail)
        {
            // Existing cart detail, add quantity not add cart detail
            var ExistingCartDetail = await _context.cartdetail
                .SingleOrDefaultAsync(cd => cd.CustomerID == CartDetail.CustomerID && cd.ShoesID == CartDetail.ShoesID);
            if(ExistingCartDetail != null) 
            { 
                var NewQuantity = ExistingCartDetail.Quantity + CartDetail.Quantity;
                ExistingCartDetail.Quantity = NewQuantity ?? 1;
                _context.cartdetail.Update(ExistingCartDetail);
                await _context.SaveChangesAsync();

                var NewCartDetail = _mapper.Map<CartDetailDTO>(ExistingCartDetail);
                return NewCartDetail;
            }

            // Add new cart detail
            var CartDetailEntity = _mapper.Map<cartdetail>(CartDetail);
            var NewCartDetailEntity = (await _context.cartdetail.AddAsync(CartDetailEntity)).Entity;
            await _context.SaveChangesAsync();
            var NewCartDettail = _mapper.Map<CartDetailDTO>(NewCartDetailEntity);

            return NewCartDettail;
        }

        public async Task<bool> RemoveShoes(CartDetailDTO CartDetail)
        {
            // Check shoes id in cart detail
            var CartDetailEntity = await _context.cartdetail
                .SingleOrDefaultAsync(cd => cd.CustomerID == CartDetail.CustomerID && cd.ShoesID == CartDetail.ShoesID);
            if (CartDetailEntity == null)
            {
                return false;
            }

            // Remove if it exist
            _context.cartdetail.Remove(CartDetailEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartDetailDTO?> UpdateShoesAmount(CartDetailDTO CartDetail)
        {
            // Check one cart detail in cart detail
            var CartDetailEntity = await _context.cartdetail
                .SingleOrDefaultAsync(cd => cd.CustomerID == CartDetail.CustomerID && cd.ShoesID == CartDetail.ShoesID);

            // Not exist
            if (CartDetailEntity == null)
            {
                return null;
            }

            // Exist, update shoes amount
            CartDetailEntity.Quantity = CartDetail.Quantity ?? 1;
            await _context.SaveChangesAsync();

            // Return
            var NewCartDetail = _mapper.Map<CartDetailDTO>(CartDetailEntity);
            return NewCartDetail;
        }
    }
}
