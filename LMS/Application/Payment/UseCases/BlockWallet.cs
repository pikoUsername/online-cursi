﻿using LMS.Application.Common.Interfaces;
using LMS.Application.Common.UseCases;
using LMS.Application.Payment.Dto;
using LMS.Domain.User.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Payment.UseCases
{
    public class BlockWallet : BaseUseCase<BlockWalletDto, bool>
    {
        private IApplicationDbContext _context;
        private readonly IAccessPolicy _accessPolicy;

        public BlockWallet(IApplicationDbContext dbContext, IAccessPolicy accessPolicy)
        {
            _context = dbContext;
            _accessPolicy = accessPolicy;
        }

        public async Task<bool> Execute(BlockWalletDto dto)
        {
            await _accessPolicy.EnforceRole(UserRoles.Admin);

            var wallet = await _context.Wallets.FirstOrDefaultAsync(x => x.Id == dto.WalletId);

            Guard.Against.Null(wallet, message: "Wallet does not exists");

            wallet.Block(dto.Reason);

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
