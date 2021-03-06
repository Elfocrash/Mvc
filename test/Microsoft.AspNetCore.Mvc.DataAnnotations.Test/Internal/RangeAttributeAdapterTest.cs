// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Validation
{
    public class RangeAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void AddValidation_WithoutLocalization()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new RangeAttribute(typeof(decimal), "0", "100");
            attribute.ErrorMessage = "The field Length must be between {1} and {2}.";

            var expectedMessage = "The field Length must be between 0 and 100.";

            var adapter = new RangeAttributeAdapter(attribute, stringLocalizer: null);

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-range", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-range-max", kvp.Key); Assert.Equal("100", kvp.Value); },
                kvp => { Assert.Equal("data-val-range-min", kvp.Key); Assert.Equal("0", kvp.Value); });
        }

        [Fact]
        [ReplaceCulture]
        public void AddValidation_WithLocalization()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new RangeAttribute(typeof(decimal), "0", "100");
            attribute.ErrorMessage = "The field Length must be between {1} and {2}.";

            var expectedProperties = new object[] { "Length", 0m, 100m };
            var expectedMessage = "The field Length must be between 0 and 100.";

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer
                .Setup(s => s[attribute.ErrorMessage, expectedProperties])
                .Returns(new LocalizedString(attribute.ErrorMessage, expectedMessage));

            var adapter = new RangeAttributeAdapter(attribute, stringLocalizer: stringLocalizer.Object);

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-range", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-range-max", kvp.Key); Assert.Equal("100", kvp.Value); },
                kvp => { Assert.Equal("data-val-range-min", kvp.Key); Assert.Equal("0", kvp.Value); });
        }

        [Fact]
        [ReplaceCulture]
        public void AddValidation_DoesNotTrounceExistingAttributes()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new RangeAttribute(typeof(decimal), "0", "100");
            attribute.ErrorMessage = "The field Length must be between {1} and {2}.";

            var adapter = new RangeAttributeAdapter(attribute, stringLocalizer: null);

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            context.Attributes.Add("data-val", "original");
            context.Attributes.Add("data-val-range", "original");
            context.Attributes.Add("data-val-range-max", "original");
            context.Attributes.Add("data-val-range-min", "original");

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-range", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-range-max", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-range-min", kvp.Key); Assert.Equal("original", kvp.Value); });
        }
    }
}
