using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppDB.Entities;
using KinoAppDB.Repository;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public class KinosaalService : IKinosaalService
    {
        private readonly IKinosaalRepository _repoKinosaal;
        private readonly ISitzreiheRepository _repoSitzreihe;
        private readonly ISitzplatzRepository _repoSitzplatz;
        private readonly IMapper _mapper;

        public KinosaalService(IKinosaalRepository repoKinosaal, ISitzreiheRepository repoSitzreihe, ISitzplatzRepository repoSitzplatz, IMapper mapper)
        {
            _repoKinosaal = repoKinosaal;
            _repoSitzreihe = repoSitzreihe;
            _repoSitzplatz = repoSitzplatz;
            _mapper = mapper;
        }

        public async Task CreateAsync(CreateKinosaalDTO kinosaal, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default)
        {
            var entity = _mapper.Map<KinosaalEntity>(kinosaal);
            await _repoKinosaal.AddAsync(entity, ct);

            for (int i = 0; i < AnzahlSitzreihen; i++)
            {
                var sitzreiheDto = new CreateSitzreiheDTO(
                    0,
                    $"Reihe {i + 1}",
                    entity.Id
                );

                var sitzreiheEntity = _mapper.Map<SitzreiheEntity>(sitzreiheDto);
                await _repoSitzreihe.AddAsync(sitzreiheEntity, ct);

                // 3. Sitzplätze für jede Sitzreihe erzeugen
                for (int s = 0; s < GrößeSitzreihen; s++)
                {
                    var sitzplatzDTO = new CreateSitzplatzDTO(
                        gebucht: false,
                        nummer: s + 1,
                        price: 10,
                        sitzreiheId: sitzreiheEntity.Id
                    );

                    var sitzEntity = _mapper.Map<SitzplatzEntity>(sitzplatzDTO);
                    await _repoSitzplatz.AddAsync(sitzEntity, ct);
                }
            }
        }

        public async Task DeleteAsync(KinosaalEntity kinosaal, CancellationToken ct = default)
        {
            await _repoKinosaal.DeleteAsync(kinosaal, ct);
        }
    }
}
