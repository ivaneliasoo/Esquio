﻿using Esquio.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Esquio.AspNetCore.Toggles
{
    /// <summary>
    /// A binary <see cref="IToggle"/> that is active depending on the current Role name and if this is contained in configured Roles property.
    /// </summary>
    [DesignType(Description = "The identity role is in the list.", FriendlyName = "Identity Role")]
    [DesignTypeParameter(ParameterName = Roles, ParameterType = EsquioConstants.SEMICOLON_LIST_PARAMETER_TYPE, ParameterDescription = "The identity role values to activate this toggle separated by ';' character.")]
    public class RoleNameToggle
       : IToggle
    {
        internal const string Roles = nameof(Roles);
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Create a new instance of <see cref="RoleNameToggle"/>.
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> service to be used.</param>
        public RoleNameToggle(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc/>
        public ValueTask<bool> IsActiveAsync(ToggleExecutionContext context, CancellationToken cancellationToken = default)
        {
            var roles = context.Data[Roles]?.ToString();

            if (roles is null)
            {
                return new ValueTask<bool>(false);
            }

            var tokenizer = new StringTokenizer(roles, EsquioConstants.DEFAULT_SPLIT_SEPARATOR);

            foreach (var role in tokenizer)
            {
                var isActive = _httpContextAccessor
                    .HttpContext?
                    .User?
                    .IsInRole(role.Value);

                if (isActive.HasValue && isActive.Value)
                {
                    return new ValueTask<bool>(true);
                }
            }

            return new ValueTask<bool>(false);
        }
    }
}
