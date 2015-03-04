﻿/* File:        ILexicalBiasAnalyser.cs
 * Purpose:     Interface for various implementations of lexical 
 *              bias analysis.
 * Created:     17th February 2015
 * Author:      Gary Fernie
 * Exposes:     ILexicalBiasAnalyser
 * 
 * Description: - Defines a method to return Opinion from a given string.
 */

namespace GroupA.FolksOpinion.UI.Models
{
    public interface ILexicalBiasAnalyser
    {
        Opinion Analyse (string text);
    }
}
