﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApplication2.Dto;
using WebApplication2.Services;
using WebApplication2.Exceptions;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterProduct([FromBody] RegisterProductRequestDTO dto)
        {
            try
            {
                var idProductWarehouse = await _warehouseService.RegisterProductInWarehouseAsync(dto);
                return Ok(new { IdProductWarehouse = idProductWarehouse });
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ConflictException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}