﻿namespace TheShoesShop_BackEnd.DTOs
{
    public class Response
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; } = null;
    }
}
