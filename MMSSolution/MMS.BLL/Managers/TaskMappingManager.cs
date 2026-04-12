using Intalio.Tools.Common.Encryptions;
using Intalio.Tools.Common.Extensions.FileExtensions;
using MapsterMapper;
using Microsoft.Data.SqlClient;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DTO;

namespace MMS.BLL.Managers
{
    public class TaskMappingManager
    {
        private readonly IMapper _mapper;

        private readonly ISettingsUnitOfWork _settingsUnitOfWork;
        public TaskMappingManager(IMapper mapper, ISettingsUnitOfWork settingsUnitOfWork)
        {
            _mapper = mapper;
            _settingsUnitOfWork = settingsUnitOfWork;
        }

        public List<DataReaderDto> ListMasterDataAsync(int mapperId, string? tableName, string? textField, string? valueField)
        {
            List<DataReaderDto> result = new();

            var datasource = _settingsUnitOfWork.DataSources.Get(x => x.Id == mapperId);
            if (datasource != null)
            {
                var connection = string.Format("server={0};uid={1};pwd={2}; Database={3}; TrustServerCertificate=True", datasource.InstanceName, datasource.Username, EncryptionService.Decrypt(datasource.Password), datasource.Dbname).Trim();
                if (!string.IsNullOrWhiteSpace(connection))
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connection))
                    {
                        SqlCommand cmd = new SqlCommand(string.Format("Select {0} from {1}", textField + "," + valueField,  tableName.GetExplicitTableName()), sqlConnection);
                        sqlConnection.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var item = new DataReaderDto
                            {
                                Name = reader[textField].ToString(),
                                Id = reader[valueField].ToString()
                            };
                            result.Add(item);
                        }
                        sqlConnection.Close();
                    }
                }
            }

            return result;
        }
    }
}
