using RadioMessagesProcessor.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace radioMessagesProcessor.Services
{
    public interface IDecoder
    {
        /// <summary>
        /// Creates the message from the raw event
        /// </summary>
        /// <param name="rawEvent"></param>
        /// <param name="isSuccessful">true when the creation succeeds with no errors.</param>
        /// <param name="error_message">error message encountered whenever the raw event cannot be parsed</param>
        /// <returns></returns>
        RadioLocationMessageDto FromRawEvent(string rawEvent, out bool isSuccessful, out string error_message);

        /// <summary>
        /// Attempts to decode the location of the neighbour towers and the approximate device location using triangulation
        /// </summary>
        /// <param name="radioLocation">the undecoded <see cref="RadioLocationMessageDto"/> message</param>
        /// <param name="error_message">errors encounters during decoding</param>
        /// <returns>true if succesfully decoded</returns>
        bool Decode(RadioLocationMessageDto radioLocation, out string error_message);
    }

    public class Decoder : IDecoder
    {
        private ICellsitesQueryService cellsitesQueryService;
        public Decoder(ICellsitesQueryService cellsitesQueryService)
        {
            this.cellsitesQueryService = cellsitesQueryService;
        }

        
        public bool Decode(RadioLocationMessageDto radioLocation, out string error_message)
        {
            throw new NotImplementedException();
        }

        
        public RadioLocationMessageDto FromRawEvent(string rawEvent, out bool isSuccessful, out string error_message)
        {
            throw new NotImplementedException();
        }
    }
}
