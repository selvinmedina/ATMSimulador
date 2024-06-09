﻿using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Servicios
{
    public class ServiciosService : IServiciosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiciosService> _logger;

        public ServiciosService(
            ILogger<ServiciosService> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<ServicioDto>> CrearServicioAsync(ServicioDto servicioDto)
        {
            var servicio = new Servicio
            {
                NombreServicio = servicioDto.NombreServicio,
                Descripcion = servicioDto.Descripcion
            };

            try
            {
                _unitOfWork.Repository<Servicio>().Add(servicio);
                await _unitOfWork.SaveAsync();

                servicioDto.ServicioId = servicio.ServicioId;
                return Response<ServicioDto>.Success(servicioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando servicio");
                return Response<ServicioDto>.Fail(ex.Message);
            }
        }

        public async Task<Response<ServicioDto>> EditarServicioAsync(ServicioDto servicioDto)
        {
            var servicio = await _unitOfWork.Repository<Servicio>().AsQueryable().FirstOrDefaultAsync(x => x.ServicioId == servicioDto.ServicioId);

            if (servicio == null)
            {
                return Response<ServicioDto>.Fail("Servicio no encontrado");
            }

            servicio.NombreServicio = servicioDto.NombreServicio;
            servicio.Descripcion = servicioDto.Descripcion;

            try
            {
                _unitOfWork.Repository<Servicio>().Update(servicio);
                await _unitOfWork.SaveAsync();

                return Response<ServicioDto>.Success(servicioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando servicio");
                return Response<ServicioDto>.Fail(ex.Message);
            }
        }

        public async Task<Response<List<ServicioDto>>> ListarServiciosAsync()
        {
            try
            {
                var servicios = await _unitOfWork.Repository<Servicio>().AsQueryable().ToListAsync();
                var serviciosDto = servicios.Select(s => new ServicioDto
                {
                    ServicioId = s.ServicioId,
                    NombreServicio = s.NombreServicio,
                    Descripcion = s.Descripcion
                }).ToList();

                return Response<List<ServicioDto>>.Success(serviciosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando servicios");
                return Response<List<ServicioDto>>.Fail(ex.Message);
            }
        }

        public async Task<Response<ServicioDto>> ListarServicioPorIdAsync(int servicioId)
        {
            var servicio = await _unitOfWork.Repository<Servicio>().AsQueryable().FirstOrDefaultAsync(x => x.ServicioId == servicioId);

            if (servicio == null)
            {
                return Response<ServicioDto>.Fail("Servicio no encontrado");
            }

            var servicioDto = new ServicioDto
            {
                ServicioId = servicio.ServicioId,
                NombreServicio = servicio.NombreServicio,
                Descripcion = servicio.Descripcion
            };

            return Response<ServicioDto>.Success(servicioDto);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
