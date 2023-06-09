﻿using Microsoft.EntityFrameworkCore;
using TheShoesShop_BackEnd.DTOs;
using TheShoesShop_BackEnd.Models;

namespace TheShoesShop_BackEnd.Services
{
    public class ShoesModelService
    {
        private readonly TheShoesShopDbContext _context;

        public ShoesModelService(TheShoesShopDbContext context) 
        { 
            _context = context;
        }

        // get shoes model list
        public async Task<IEnumerable<ShoesModelDTO>> GetShoesModelList(
            int PageIndex, int ItemPerPage,
            string? Search,
            int? Size, string? Color,
            int? From, int? To,
            int? BrandID,
            int? Sort // 1: min price, 2: max price, else: default
        )
        {
            // Filting
            var ShoesModelListQuery =
                     (from sm in _context.shoesmodel
                      join b in _context.brand on sm.BrandID equals b.BrandID
                      where Search == null || sm.ShoesModelName.Contains(Search)
                      where BrandID == null || b.BrandID == BrandID
                      //join s in _context.shoes on sm.ShoesModelID equals s.ShoesModelID
                      //where Size == null || s.Size == Size
                      //where Color == null || s.Color == Color
                      //where From == null || s.UnitPrice >= From
                      //where To == null || s.UnitPrice <= To
                      select new ShoesModelDTO
                      {
                          ShoesModelID = sm.ShoesModelID,
                          ShoesModelName = sm.ShoesModelName,
                          ShoesModelDescription = sm.ShoesModelDescription,
                          ShoesModelStatus = sm.ShoesModelStatus,
                          Brand = new BrandDTO { BrandID = b.BrandID, BrandName = b.BrandName },
                          Images = (from i in _context.shoesmodelimage
                                    where i.ShoesModelID == sm.ShoesModelID
                                    select new ShoesModelImageDTO { ImageID = i.ImageID, ImageLink = i.ImageLink }).ToList(),
                          Shoeses = (from s in _context.shoes
                                     where s.ShoesModelID == sm.ShoesModelID
                                     where Size == null || s.Size == Size
                                     where Color == null || s.Color == Color
                                     where From == null || s.UnitPrice >= From
                                     where To == null || s.UnitPrice <= To
                                     select new ShoesDTO
                                     {
                                         ShoesID = s.ShoesID,
                                         Color = s.Color,
                                         Quantity = s.Quantity,
                                         Size = s.Size,
                                         UnitPrice = s.UnitPrice,
                                         ShoesStatus = s.ShoesStatus,
                                     }).ToList(),
                      });

            var ShoesModelFiltList = from sm in ShoesModelListQuery
                                     where sm.Shoeses!.Count() > 0
                                     //orderby (Sort == 1 || Sort == 2) ? s.UnitPrice : sm.ShoesModelID ascending
                                     select sm;
            //// Sorting, code later
            //if (Sort == 2)
            //{
            //    // Reverse list
            //    ShoesModelListQuery = ShoesModelListQuery.Reverse();
            //}

            // Paging
            var ShoesModelList = await ShoesModelFiltList
                                        .Skip((PageIndex - 1) * ItemPerPage)
                                        .Take(ItemPerPage).ToListAsync();

     
            return ShoesModelList;
        }

        // get total shoes model list
        public async Task<int?> GetShoesModelCount()
        {
            var TotalShoesModel = await _context.shoesmodel.CountAsync();
            return TotalShoesModel;
        }

        // get one shoes model by id
        public async Task<ShoesModelDTO?> GetShoesModelByID(int ShoesModelID)
        {
            var ShoesModel = await (
                             from sm in _context.shoesmodel
                             where sm.ShoesModelID == ShoesModelID
                             join b in _context.brand on sm.BrandID equals b.BrandID
                             select new ShoesModelDTO
                             {
                                 ShoesModelID = sm.ShoesModelID,
                                 ShoesModelName = sm.ShoesModelName,
                                 ShoesModelDescription = sm.ShoesModelDescription,
                                 ShoesModelStatus = sm.ShoesModelStatus,
                                 Brand = new BrandDTO { BrandID = b.BrandID, BrandName = b.BrandName },
                                 Images = (from i in _context.shoesmodelimage
                                           where i.ShoesModelID == sm.ShoesModelID
                                           select new ShoesModelImageDTO { ImageID = i.ImageID, ImageLink = i.ImageLink}).ToList(),
                                 Shoeses = (from s in _context.shoes
                                            where s.ShoesModelID == sm.ShoesModelID
                                            select new ShoesDTO
                                            {
                                                ShoesID = s.ShoesID,
                                                Color = s.Color,
                                                Quantity = s.Quantity,
                                                Size = s.Size,
                                                UnitPrice = s.UnitPrice,
                                                ShoesStatus = s.ShoesStatus,
                                            }).ToList(),
                             }).FirstOrDefaultAsync();

            return ShoesModel;
        }


    }
}
