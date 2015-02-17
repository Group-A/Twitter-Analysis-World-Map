/* File:        ILexicalBiasAnalyser.cs
 * Purpose:     Interface for various implementations of lexical 
 *              bias analysis.
 * Version:     1.0
 * Created:     17th February 2015
 * Author:      Gary Fernie
 * Exposes:     ILexicalBiasAnalyser
 * 
 * Description: - Defines a method to return Opinion from a given string.
 */

namespace GroupA.FolksOpinion.UI.Models
{
    interface ILexicalBiasAnalyser
    {
        public Opinion Analyse (string text);
    }
}
