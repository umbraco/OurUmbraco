using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Core;

namespace OurUmbraco.Auth
{
    /// <summary>
    /// Reads/Writes/Deletes values in the <see cref="ProjectAuthKey"/> database table
    /// </summary>
    public class ProjectAuthKeyService
    {
        public ProjectAuthKeyService()
            : this (ApplicationContext.Current.DatabaseContext)
        {
        }

        public ProjectAuthKeyService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;

        /// <summary>
        /// Try & see if we can find a record in the DB based off the User ID
        /// </summary>
        /// <param name="identityId">The user or member ID to try and find in the DB</param>
        /// <returns>Returns Auth Token record/object in DB or null if not found</returns>
        public ProjectAuthKey GetAuthKey(int memberId, int projectId)
        {
            //Try & find a record in the DB from the userId
            var findRecord = _dbContext.Database.SingleOrDefault<ProjectAuthKey>("WHERE MemberId=@0 AND ProjectId=@1", memberId, projectId);

            //Return the object (Will be null if can't find an item)
            return findRecord;
        }
        
        public List<ProjectAuthKey> GetAllAuthKeysForProject(int projectId)
        {
            //Try & find all records in the DB from the projectId
            var findRecords = _dbContext.Database.Fetch<ProjectAuthKey>("WHERE ProjectId=@0", projectId);

            //Return the object (Will be null if can't find an item)
            return findRecords;
        }

        /// <summary>
        /// Create a new auth key for the member/project which will generate a 256 bit random key
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public ProjectAuthKey CreateAuthKey(int memberId, int projectId, string description = "")
        {
            var existing = GetAuthKey(memberId, projectId);
            if (existing != null)
                throw new InvalidOperationException($"An auth key already exists for the member {memberId} and {projectId}");
            
            var key = new ProjectAuthKey
            {
                DateCreated = DateTime.UtcNow,
                AuthKey = GenerateKey(32),// generate a 256 bit random key
                MemberId = memberId,
                ProjectId = projectId,
                Description = description
            };

            _dbContext.Database.Save(key);

            return key;
        }

        /// <summary>
        /// Insert a new Auth Token into the custom DB table OR
        /// Update just the auth token if we find a record for the backoffice user already
        /// </summary>
        /// <param name="authKey"></param>
        public void UpdateAuthKey(ProjectAuthKey authKey)
        {
            if (authKey is null)
                throw new ArgumentNullException(nameof(authKey));

            if (authKey.PrimaryKey == default)
                throw new ArgumentException("The auth key instance has no primary key", nameof(authKey));

            _dbContext.Database.Save(authKey);
        }


        /// <summary>
        /// Deletes the auth key for the member/project
        /// </summary>
        /// <param name="identityId"></param>
        public void DeleteAuthKey(int memberId, int projectId)
        {
            //Just to be 100% sure for data sanity that a record for the user does not exist already
            var existingRecord = GetAuthKey(memberId, projectId);

            if (existingRecord != null)
            {
                //We found the record in the DB - let's remove/delete it
                _dbContext.Database.Delete<ProjectAuthKey>("WHERE MemberId=@0 AND ProjectId=@1", memberId, projectId);
            }
        }


        /// <summary>
        /// Generates a random key with the specified byte length
        /// </summary>
        /// <param name="bytelength"></param>
        /// <returns></returns>
        private static string GenerateKey(int bytelength)
        {
            byte[] buff = new byte[bytelength];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buff);
                var sb = new StringBuilder(bytelength * 2);
                for (int i = 0; i < buff.Length; i++)
                    sb.Append(string.Format("{0:X2}", buff[i]));
                return sb.ToString();
            }
        }
    }
}
