﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LMS.Domain.Files.Entities;
using LMS.Domain.Payment.ValueObjects;
using LMS.Domain.User.Entities;
using LMS.Domain.Market.Enums;
using LMS.Domain.Market.Events;
using LMS.Domain.Market.Exceptions;

namespace LMS.Domain.Market.Entities
{
    public class ProductEntity : BaseAuditableEntity
    {
        // Часть смыслового ядра, нужны тесты!! 
        [Required]
        public UserEntity CreatedBy { get; set; } = null!;
        [Required]
        public CategoryEntity Category { get; set; } = null!;

        [Required, MaxLength(256)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(256)]
        public string Slug { get; set; } = null!;

        // Assuming Money is a decimal. If Money is a complex type, adjust accordingly.
        public Money? DiscountPrice { get; set; }
        [Required]
        public Money BasePrice { get; set; } = new Money(0);
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public GameEntity Game { get; set; } = null!;

        [Required]
        public ProductStatus Status { get; set; }

        public UserEntity? BuyerUser { get; set; }

        [Column(TypeName = "jsonb")]
        public string Attributes { get; set; } = null!;
        [Required]
        public ICollection<FileEntity> Images { get; set; } = [];
        [NotMapped, Required]
        public int ProductViewed { get; set; }


        [NotMapped, Required]
        public Money CurrentPrice
        {
            get
            {
                if (DiscountPrice is null)
                {
                    return BasePrice;
                }
                else
                {
                    return DiscountPrice;
                }
            }
        }

        public static ProductEntity Create(
            UserEntity byUser,
            CategoryEntity category,
            GameEntity game,
            string name,
            Money price,
            string description,
            Dictionary<string, string> attributes,
            ICollection<FileEntity> images,
            ProductStatus status = ProductStatus.Processing)
        {
            ProductEntity newProduct = new ProductEntity()
            {
                CreatedBy = byUser,
                Category = category,
                Name = name,
                BasePrice = price,
                Description = description,
                Game = game,
                Attributes = "{}",
                Status = status,
                ProductViewed = 0,
                Images = images,
                Slug = Guid.NewGuid().ToString(),
            };

            VerifyAttributes(attributes, category);

            // TODO: Possible security breach?
            newProduct.Attributes = JsonSerializer.Serialize(attributes);

            newProduct.AddDomainEvent(
                new ProductCreated(newProduct));

            return newProduct;
        }

        public ProductViewEntity CreateView(
            UserEntity byUser)
        {
            var view = new ProductViewEntity(userId: byUser.Id, productId: Id, info: "{}");

            AddDomainEvent(new ProductViewed(this));
            return view;
        }

        public static void VerifyAttributes(
            Dictionary<string, string> attributes,
            CategoryEntity category)
        {
            List<string> fields = category.Options
                .Select(x => x.Field)
                .ToList();
            foreach (var attribute in attributes)
            {
                if (!fields.Contains(attribute.Key))
                {
                    throw new IncorrectAttributes(attribute.Key, attribute.Value);
                }
            }
        }

        public void Sold(ReviewEntity review)
        {
            if (review.Product.Id != Id)
            {
                throw new Exception("Critical, bug! Review product id is not bound to product");
            }
            Status = ProductStatus.Sold;
            AddDomainEvent(new ProductSold(this, DateTime.UtcNow));
        }
    }
}