using System;
using System.Collections.Generic;
using System.Text;

namespace CoreDefinitions
{
    public interface IOptionalAlgorithmDefinition
    {
        ref readonly byte GetTransitionA(in byte vertex);
        ref readonly byte GetTransitionB(in byte vertex);
        /// <param name="vertex">indeks wierzchołka, może być większy niż rozmiar automatu (n), więc trzeba zadbać o obsługę zwracając FALSE</param>
        bool IsDefinedVertex(in byte vertex);
        int GetMaxCapacitySize();
    }
}
