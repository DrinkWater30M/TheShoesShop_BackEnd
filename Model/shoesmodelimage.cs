﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace TheShoesShop_BackEnd.Model;

public partial class shoesmodelimage
{
    public int ImageID { get; set; }

    public int? ShoesModelID { get; set; }

    public string ImageLink { get; set; }

    public virtual shoesmodel ShoesModel { get; set; }
}