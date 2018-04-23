﻿using System;
using Bit.Core.Models.Table;
using System.Collections.Generic;
using Bit.Core.Models.Data;
using System.Linq;

namespace Bit.Core.Models.Api
{
    public class CollectionResponseModel : ResponseModel
    {
        public CollectionResponseModel(Collection collection, string obj = "collection")
            : base(obj)
        {
            if(collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            Id = collection.Id.ToString();
            OrganizationId = collection.OrganizationId.ToString();
            Name = collection.Name;
        }

        public string Id { get; set; }
        public string OrganizationId { get; set; }
        public string Name { get; set; }
    }

    public class CollectionDetailsResponseModel : CollectionResponseModel
    {
        public CollectionDetailsResponseModel(Collection collection, IEnumerable<SelectionReadOnly> groups)
            : base(collection, "collectionDetails")
        {
            Groups = groups.Select(g => new SelectionReadOnlyResponseModel(g));
        }

        public IEnumerable<SelectionReadOnlyResponseModel> Groups { get; set; }
    }
}
