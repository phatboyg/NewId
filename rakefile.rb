COPYRIGHT = "Copyright 2015 Chris Patterson, All rights reserved."

include FileTest
require 'albacore'
require 'semver'

PRODUCT = 'NewId'
CLR_TOOLS_VERSION = 'v4.0.30319'
OUTPUT_PATH = 'bin/Release'

props = {
  :src => File.expand_path("src"),
  :output => File.expand_path("build_output"),
  :artifacts => File.expand_path("build_artifacts"),
  :projects => ["NewId"],
  :lib => File.expand_path("lib")
}

desc "**Default**, compiles and runs tests"
task :default => [:clean, :nuget_restore, :compile, :tests, :nuget, :package]

desc "Update the common version information for the build. You can call this task without building."
assemblyinfo :global_version do |asm|
  # Assembly file config
  asm.product_name = PRODUCT
  asm.description = "NewId is an ordered 128-bit unique identifier generator using the Flake algorithm."
  asm.version = FORMAL_VERSION
  asm.file_version = FORMAL_VERSION
  asm.custom_attributes :AssemblyInformationalVersion => "#{BUILD_VERSION}",
    :ComVisibleAttribute => false,
    :CLSCompliantAttribute => true
  asm.copyright = COPYRIGHT
  asm.output_file = 'src/SolutionVersion.cs'
  asm.namespaces "System", "System.Reflection", "System.Runtime.InteropServices"
end

desc "Prepares the working directory for a new build"
task :clean do
	FileUtils.rm_rf props[:output]
	waitfor { !exists?(props[:output]) }

	FileUtils.rm_rf props[:artifacts]
	waitfor { !exists?(props[:artifacts]) }

	Dir.mkdir props[:output]
	Dir.mkdir props[:artifacts]
end

desc "Cleans, versions, compiles the application and generates build_output/."
task :compile => [:versioning, :global_version, :build] do
	copyOutputFiles File.join(props[:src], "NewId/bin/Release"), "NewId.{dll,pdb,xml}", File.join(props[:output], 'net-4.5')
end

desc "Only compiles the application."
msbuild :build do |msb|
	msb.properties :Configuration => "Release",
		:Platform => 'Any CPU'
	msb.use :net4
	msb.targets :Clean, :Build
	msb.solution = 'src/NewId.sln'
end

def copyOutputFiles(fromDir, filePattern, outDir)
	FileUtils.mkdir_p outDir unless exists?(outDir)
	Dir.glob(File.join(fromDir, filePattern)){|file|
		copy(file, outDir) if File.file?(file)
	}
end

desc "Runs unit tests"
nunit :tests => [:compile] do |nunit|
  nunit.command = File.join(props[:src], 'packages','NUnit.Runners.2.6.4', 'tools', 'nunit-console.exe')
  nunit.options = "/framework=#{CLR_TOOLS_VERSION}", '/nothread', '/nologo', '/labels', "\"/xml=#{File.join(props[:artifacts], 'nunit-test-results.xml')}\""
  nunit.assemblies = FileList[File.join(props[:src], "NewId.Tests/bin/Release", "NewId.Tests.dll")]
end

task :package => [:nuget]

desc "ZIPs up the build results."
zip :zip_output do |zip|
  zip.directories_to_zip = [props[:stage]]
  zip.output_file = "NewId-#{NUGET_VERSION}.zip"
  zip.output_path = [props[:artifacts]]
end

desc "Builds the nuget package"
task :nuget => ['create_nuspec'] do
  sh "#{File.join(props[:src],'.nuget','nuget.exe')} pack #{props[:artifacts]}/NewId.nuspec /Symbols /OutputDirectory #{props[:artifacts]}"
end

desc "restores missing packages"
msbuild :nuget_restore do |msb|
  msb.use :net4
  msb.targets :RestorePackages
  msb.solution = File.join(props[:src], "NewId.Tests", "NewId.Tests.csproj")
end

nuspec :create_nuspec do |nuspec|
  nuspec.id = 'NewId'
  nuspec.version = NUGET_VERSION
  nuspec.authors = 'Chris Patterson'
  nuspec.description = 'NewId is an ordered 128-bit unique identifier generator.'
  nuspec.title = 'NewId'
  nuspec.projectUrl = 'http://github.com/PhatBoyG/NewId'
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0"
  nuspec.requireLicenseAcceptance = "false"
  nuspec.output_file = File.join(props[:artifacts], 'NewId.nuspec')
  add_files props[:output], 'NewId.{dll,pdb,xml}', nuspec
  nuspec.file(File.join(props[:src], "NewId\\**\\*.cs").gsub("/","\\"), "src")
end

def project_outputs(props)
  props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.dll" }.
    concat( props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.exe" } ).
    find_all{ |path| exists?(path) }
end

def add_files stage, what_dlls, nuspec
  [['net45', 'net-4.5']].each{|fw|
    takeFrom = File.join(stage, fw[1], what_dlls)
    puts "Taking #{takeFrom}"
    Dir.glob(takeFrom).each do |f|
      puts "File: #{f}, #{fw[0]}"
      nuspec.file(f.gsub("/", "\\"), "lib\\#{fw[0]}")
    end
  }
end

def commit_data
  begin
    commit = `git rev-parse --short HEAD`.chomp()[0,6]
    git_date = `git log -1 --date=iso --pretty=format:%ad`
    commit_date = DateTime.parse( git_date ).strftime("%Y-%m-%d %H%M%S")
  rescue Exception => e
    puts e.inspect
    commit = (ENV['BUILD_VCS_NUMBER'] || "000000")[0,6]
    commit_date = Time.new.strftime("%Y-%m-%d %H%M%S")
  end
  [commit, commit_date]
end

task :versioning do
  ver = SemVer.find
  revision = (ENV['BUILD_NUMBER'] || ver.patch).to_i
  var = SemVer.new(ver.major, ver.minor, revision, ver.special)
  
  # extensible number w/ git hash
  ENV['BUILD_VERSION'] = BUILD_VERSION = ver.format("%M.%m.%p%s") + ".#{commit_data()[0]}"
  
  # nuget (not full semver 2.0.0-rc.1 support) see http://nuget.codeplex.com/workitem/1796
  ENV['NUGET_VERSION'] = NUGET_VERSION = ver.format("%M.%m.%p%s")
  
  # purely M.m.p format
  ENV['FORMAL_VERSION'] = FORMAL_VERSION = "#{ SemVer.new(ver.major, ver.minor, revision).format "%M.%m.%p"}"
  puts "##teamcity[buildNumber '#{BUILD_VERSION}']" # tell teamcity our decision
end

def waitfor(&block)
  checks = 0

  until block.call || checks >10
    sleep 0.5
    checks += 1
  end

  raise 'Waitfor timeout expired. Make sure that you aren\'t running something from the build output folders, or that you have browsed to it through Explorer.' if checks > 10
end
