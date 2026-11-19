using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Medicines;

namespace MediReminderBackend.Interfaces;

public interface IMedicineService
{
    Task<ApiResponse<MedicineListResponseDto>> GetMedicinesAsync(int userId, string? status, string? search);
    Task<ApiResponse<MedicineDto>> GetMedicineByIdAsync(int userId, int medicineId);
    Task<ApiResponse<List<MedicineDto>>> CreateMedicinesAsync(int userId, List<CreateMedicineDto> dtos);
    Task<ApiResponse<MedicineDto>> UpdateMedicineAsync(int userId, int medicineId, UpdateMedicineDto dto);
    Task<ApiResponse<bool>> DeleteMedicineAsync(int userId, int medicineId);
}
