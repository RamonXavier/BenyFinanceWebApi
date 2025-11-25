using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenyFinance.Application.DTOs;
using BenyFinance.Application.Interfaces;
using BenyFinance.Domain.Entities;
using BenyFinance.Domain.Interfaces;

namespace BenyFinance.Application.Services;

public class CreditCardService : ICreditCardService
{
    private readonly ICreditCardRepository _creditCardRepository;

    public CreditCardService(ICreditCardRepository creditCardRepository)
    {
        _creditCardRepository = creditCardRepository;
    }

    public async Task<IEnumerable<CreditCardDto>> GetAllAsync(Guid userId)
    {
        var cards = await _creditCardRepository.GetAllByUserIdAsync(userId);
        return cards.Select(c => new CreditCardDto(c.Id, c.Name, c.Limit, c.Bank, c.ClosingDay, c.DueDay));
    }

    public async Task<CreditCardDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var c = await _creditCardRepository.GetByIdAsync(id);
        if (c == null || c.UserId != userId)
        {
            return null;
        }
        return new CreditCardDto(c.Id, c.Name, c.Limit, c.Bank, c.ClosingDay, c.DueDay);
    }

    public async Task<CreditCardDto> CreateAsync(Guid userId, CreateCreditCardDto createDto)
    {
        var card = new CreditCard
        {
            Name = createDto.Name,
            Limit = createDto.Limit,
            Bank = createDto.Bank,
            ClosingDay = createDto.ClosingDay,
            DueDay = createDto.DueDay,
            UserId = userId
        };

        await _creditCardRepository.AddAsync(card);
        return new CreditCardDto(card.Id, card.Name, card.Limit, card.Bank, card.ClosingDay, card.DueDay);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var card = await _creditCardRepository.GetByIdAsync(id);
        if (card == null || card.UserId != userId)
        {
            return false;
        }

        await _creditCardRepository.DeleteAsync(card);
        return true;
    }
}
