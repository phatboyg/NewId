﻿namespace MassTransit.NewIdTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NewIdFormatters;
    using NewIdParsers;
    using NUnit.Framework;


    [TestFixture]
    public class Using_the_newid_formatters
    {
        [Test]
        public void Should_compare_known_conversions()
        {
#if NETCORE
            string directory = AppContext.BaseDirectory;
#else
            string directory = AppDomain.CurrentDomain.BaseDirectory;
#endif

            string newIdFileName = Path.Combine(directory, "guids.txt");
            string textsFileName = Path.Combine(directory, "texts.txt");

            var newIds = new List<NewId>();

            using (FileStream file = File.Open(newIdFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    newIds.Add(new NewId(line.Trim()));
                }
            }

            var texts = new List<string>(newIds.Count);

            using (FileStream file = File.Open(textsFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    texts.Add(line.Trim());
                }
            }

            Assert.AreEqual(newIds.Count, texts.Count);

            var formatter = new Base32Formatter("0123456789ABCDEFGHIJKLMNOPQRSTUV");

            for (int i = 0; i < newIds.Count; i++)
            {
                string text = newIds[i].ToString(formatter);

                Assert.AreEqual(texts[i], text);
            }

            Console.WriteLine("Compared {0} equal conversions", texts.Count);
        }

        [Test]
        public void Should_convert_back_using_parser()
        {
            var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

            var formatter = new ZBase32Formatter(true);

            string ns = n.ToString(formatter);

            var parser = new ZBase32Parser();
            NewId newId = parser.Parse(ns);


            Assert.AreEqual(n, newId);
        }

        [Test]
        public void Should_convert_back_using_standard_parser()
        {
            var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

            var formatter = new Base32Formatter(true);

            string ns = n.ToString(formatter);

            var parser = new Base32Parser();
            NewId newId = parser.Parse(ns);


            Assert.AreEqual(n, newId);
        }

        [Test]
        public void Should_convert_using_custom_base32_formatting_characters()
        {
            var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

            var formatter = new Base32Formatter("0123456789ABCDEFGHIJKLMNOPQRSTUV");

            string ns = n.ToString(formatter);

            Assert.AreEqual("UQP7OV4AN129HB4N79GGF8GJ10", ns);
        }

        [Test]
        public void Should_convert_using_standard_base32_formatting_characters()
        {
            var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

            var formatter = new Base32Formatter(true);

            string ns = n.ToString(formatter);

            Assert.AreEqual("62ZHY7EKXBCJRLEXHJQQPIQTBA", ns);
        }

        [Test]
        public void Should_convert_using_the_optimized_human_readable_formatter()
        {
            var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

            var formatter = new ZBase32Formatter(true);

            string ns = n.ToString(formatter);

            Assert.AreEqual("6438A9RKZBNJTMRZ8JOOXEOUBY", ns);
        }

        [Test]
        public void Should_translate_often_transposed_characters_to_proper_values()
        {
            var n = new NewId("F6B27C7C-8AB8-4498-AC97-3A6107A21320");

            string ns = "6438A9RK2BNJTMRZ8J0OXE0UBY";

            var parser = new ZBase32Parser(true);
            NewId newId = parser.Parse(ns);


            Assert.AreEqual(n, newId);
        }
    }
}